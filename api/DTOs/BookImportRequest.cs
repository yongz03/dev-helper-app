using System.ComponentModel.DataAnnotations;

namespace DevHelper.Api.DTOs;

public sealed class BookImportRequest
{
    [Required, StringLength(2048, MinimumLength = 1)]
    public string FolderPath { get; init; } = string.Empty;
}
