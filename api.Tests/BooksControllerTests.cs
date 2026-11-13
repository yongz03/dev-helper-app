using DevHelper.Api.Controllers;
using DevHelper.Api.Data;
using DevHelper.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevHelper.Api.Services;
using Xunit;

namespace DevHelper.Api.Tests;

public sealed class BooksControllerTests
{
    private sealed class RecordingBookLauncher : IBookLauncher
    {
        public string? OpenedFilePath { get; private set; }
        public void Open(string filePath) => OpenedFilePath = filePath;
    }

    private static AppDbContext CreateContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task CreateBook_ValidRequest_AddsBookAndReturnsCreated()
    {
        await using var context = CreateContext();
        var controller = new BooksController(context, new RecordingBookLauncher());
        var result = await controller.CreateBook(new BookRequest { Name = " Dune ", FileType = ".pdf", FilePath = " C:\\Books\\dune.pdf " }, default);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<BookResponse>(created.Value);
        Assert.Equal("Dune", response.Name);
        Assert.Equal("PDF", response.FileType);
        Assert.Equal("C:\\Books\\dune.pdf", response.FilePath);
        Assert.Single(context.Books);
    }

    [Fact]
    public async Task GetBooks_ReturnsBooksOrderedByName()
    {
        await using var context = CreateContext();
        context.Books.AddRange(
            new() { Name = "Zodiac", FileType = "EPUB", FilePath = "C:\\zodiac.epub" },
            new() { Name = "Dune", FileType = "PDF", FilePath = "C:\\dune.pdf" });
        await context.SaveChangesAsync();
        var controller = new BooksController(context, new RecordingBookLauncher());
        var result = await controller.GetBooks(default);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var books = Assert.IsType<List<BookResponse>>(ok.Value);
        Assert.Equal(new[] { "Dune", "Zodiac" }, books.Select(book => book.Name));
    }

    [Fact]
    public async Task UpdateBook_ValidRequest_UpdatesBook()
    {
        await using var context = CreateContext();
        var book = new DevHelper.Api.Models.Book { Name = "Old", FileType = "PDF", FilePath = "C:\\old.pdf" };
        context.Books.Add(book);
        await context.SaveChangesAsync();
        var controller = new BooksController(context, new RecordingBookLauncher());
        var result = await controller.UpdateBook(book.Id,
            new BookRequest { Name = "New title", FileType = "epub", FilePath = "D:\\Books\\new.epub" }, default);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<BookResponse>(ok.Value);
        Assert.Equal("New title", response.Name);
        Assert.Equal("EPUB", response.FileType);
        Assert.Equal("D:\\Books\\new.epub", response.FilePath);
    }

    [Fact]
    public async Task UpdateBook_MissingBook_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var controller = new BooksController(context, new RecordingBookLauncher());
        var result = await controller.UpdateBook(42,
            new BookRequest { Name = "Dune", FileType = "pdf", FilePath = "C:\\dune.pdf" }, default);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task OpenBook_ExistingBook_LaunchesItsSavedPath()
    {
        await using var context = CreateContext();
        var book = new DevHelper.Api.Models.Book { Name = "Dune", FileType = "PDF", FilePath = "C:\\Books\\dune.pdf" };
        context.Books.Add(book);
        await context.SaveChangesAsync();
        var launcher = new RecordingBookLauncher();
        var controller = new BooksController(context, launcher);

        var result = await controller.OpenBook(book.Id, default);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(book.FilePath, launcher.OpenedFilePath);
    }

    [Fact]
    public async Task OpenBook_UsesBookNameForFileNameAndKeepsItsFolderAndType()
    {
        await using var context = CreateContext();
        var book = new DevHelper.Api.Models.Book
        {
            Name = "野蛮大陆",
            FileType = "MOBI",
            FilePath = @"D:\Users\Yong Zhang\Documents\eBooks\history\History.mobi"
        };
        context.Books.Add(book);
        await context.SaveChangesAsync();
        var launcher = new RecordingBookLauncher();
        var controller = new BooksController(context, launcher);

        var result = await controller.OpenBook(book.Id, default);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(@"D:\Users\Yong Zhang\Documents\eBooks\history\野蛮大陆.mobi", launcher.OpenedFilePath);
        Assert.Equal(launcher.OpenedFilePath, (await context.Books.FindAsync(book.Id))?.FilePath);
    }

    [Fact]
    public async Task OpenBook_MissingBook_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var controller = new BooksController(context, new RecordingBookLauncher());

        var result = await controller.OpenBook(42, default);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ImportBooks_AddsSupportedFilesRecursivelyAndSkipsDuplicates()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"dev-helper-books-{Guid.NewGuid():N}");
        var nestedFolder = Path.Combine(folder, "History");
        Directory.CreateDirectory(nestedFolder);
        var mobiPath = Path.Combine(nestedFolder, "野蛮大陆.mobi");
        var epubPath = Path.Combine(folder, "Dune.epub");
        var unsupportedPath = Path.Combine(folder, "notes.txt");
        await File.WriteAllTextAsync(mobiPath, "book");
        await File.WriteAllTextAsync(epubPath, "book");
        await File.WriteAllTextAsync(unsupportedPath, "ignore");

        try
        {
            await using var context = CreateContext();
            context.Books.Add(new DevHelper.Api.Models.Book { Name = "Existing", FileType = "MOBI", FilePath = mobiPath });
            await context.SaveChangesAsync();
            var controller = new BooksController(context, new RecordingBookLauncher());

            var result = await controller.ImportBooks(new BookImportRequest { FolderPath = folder }, default);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var summary = Assert.IsType<BookImportResponse>(ok.Value);
            Assert.Equal(1, summary.ImportedCount);
            Assert.Equal(1, summary.SkippedCount);
            Assert.Contains(context.Books, book => book.Name == "Dune" && book.FileType == "EPUB" && book.FilePath == epubPath);
            Assert.DoesNotContain(context.Books, book => book.FilePath == unsupportedPath);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
