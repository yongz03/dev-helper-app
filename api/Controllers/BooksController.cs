using DevHelper.Api.Data;
using DevHelper.Api.DTOs;
using DevHelper.Api.Models;
using DevHelper.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace DevHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BooksController(AppDbContext dbContext, IBookLauncher bookLauncher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookResponse>>> GetBooks(CancellationToken cancellationToken)
    {
        var books = await dbContext.Books.AsNoTracking()
            .OrderBy(book => book.Name)
            .Select(book => new BookResponse(book.Id, book.Name, book.FileType, book.FilePath))
            .ToListAsync(cancellationToken);
        return Ok(books);
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> CreateBook(BookRequest request, CancellationToken cancellationToken)
    {
        if (!TryNormalize(request, out var name, out var fileType, out var filePath))
        {
            return ValidationProblem(ModelState);
        }

        var book = new Book
        {
            Name = name,
            FileType = fileType,
            FilePath = filePath
        };
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);
        var response = new BookResponse(book.Id, book.Name, book.FileType, book.FilePath);
        return CreatedAtAction(nameof(GetBooks), response);
    }

    [HttpPost("import")]
    public async Task<ActionResult<BookImportResponse>> ImportBooks(BookImportRequest request, CancellationToken cancellationToken)
    {
        var folderPath = request.FolderPath.Trim();
        if (folderPath.Length == 0 || !Directory.Exists(folderPath))
        {
            ModelState.AddModelError(nameof(request.FolderPath), "The selected folder does not exist.");
            return ValidationProblem(ModelState);
        }

        try
        {
            var knownPaths = await dbContext.Books.AsNoTracking()
                .Select(book => book.FilePath)
                .ToListAsync(cancellationToken);
            var uniquePaths = new HashSet<string>(knownPaths, StringComparer.OrdinalIgnoreCase);
            var importedCount = 0;
            var skippedCount = 0;

            foreach (var filePath in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant();
                if (extension is not ("PDF" or "EPUB" or "MOBI")) continue;
                if (!uniquePaths.Add(filePath))
                {
                    skippedCount++;
                    continue;
                }

                dbContext.Books.Add(new Book
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    FileType = extension,
                    FilePath = filePath
                });
                importedCount++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new BookImportResponse(importedCount, skippedCount));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return Problem(title: "Could not read the selected folder", detail: exception.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookResponse>> UpdateBook(int id, BookRequest request, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.FindAsync([id], cancellationToken);
        if (book is null) return NotFound();

        if (!TryNormalize(request, out var name, out var fileType, out var filePath))
        {
            return ValidationProblem(ModelState);
        }

        book.Name = name;
        book.FileType = fileType;
        book.FilePath = filePath;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new BookResponse(book.Id, book.Name, book.FileType, book.FilePath));
    }

    [HttpPost("{id:int}/open")]
    public async Task<IActionResult> OpenBook(int id, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (book is null) return NotFound();

        try
        {
            var directory = Path.GetDirectoryName(book.FilePath);
            var extension = Path.GetExtension(book.FilePath);
            if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(extension))
            {
                book.FilePath = Path.Combine(directory, $"{book.Name}{extension}");
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            bookLauncher.Open(book.FilePath);
            return NoContent();
        }
        catch (FileNotFoundException)
        {
            return NotFound(new ProblemDetails { Title = "Book file not found", Detail = "Check that the saved local file path exists." });
        }
        catch (Exception exception) when (exception is InvalidOperationException or Win32Exception or ArgumentException)
        {
            return Problem(
                title: "Could not open the book",
                detail: $"Windows could not open '{book.FilePath}'. Check the file association for .{book.FileType.ToLowerInvariant()}. {exception.Message}");
        }
    }

    private bool TryNormalize(BookRequest request, out string name, out string fileType, out string filePath)
    {
        name = request.Name.Trim();
        fileType = request.FileType.Trim().TrimStart('.').ToUpperInvariant();
        filePath = request.FilePath.Trim();
        var isValid = true;
        if (name.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Name), "Book name is required.");
            isValid = false;
        }
        if (fileType.Length == 0)
        {
            ModelState.AddModelError(nameof(request.FileType), "File type is required.");
            isValid = false;
        }
        if (filePath.Length == 0)
        {
            ModelState.AddModelError(nameof(request.FilePath), "Local file path is required.");
            isValid = false;
        }
        return isValid;
    }
}
