using System.ComponentModel.DataAnnotations;

namespace DevHelper.Api.DTOs;

public sealed class BookRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(20)]
    public string FileType { get; init; } = string.Empty;

    [Required, StringLength(2048)]
    public string FilePath { get; init; } = string.Empty;
}
