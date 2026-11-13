namespace DevHelper.Api.Models;

public sealed class Book
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string FileType { get; set; }
    public required string FilePath { get; set; }
}
