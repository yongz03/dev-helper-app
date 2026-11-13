using System.Diagnostics;

namespace DevHelper.Api.Services;

public sealed class BookLauncher : IBookLauncher
{
    public void Open(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The book file could not be found.", fullPath);
        }

        Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
    }
}
