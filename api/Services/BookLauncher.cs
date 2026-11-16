using System.Diagnostics;
using System.ComponentModel;

namespace DevHelper.Api.Services;

public sealed class BookLauncher(IBookProcessStarter processStarter) : IBookLauncher
{
    public void Open(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The book file could not be found.", fullPath);
        }

        if (string.Equals(Path.GetExtension(fullPath), ".epub", StringComparison.OrdinalIgnoreCase))
        {
            var viewerPath = FindCalibreViewer();
            var startInfo = new ProcessStartInfo(viewerPath) { UseShellExecute = false };
            startInfo.ArgumentList.Add(fullPath);
            processStarter.Start(startInfo);
            return;
        }

        processStarter.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
    }

    private static string FindCalibreViewer()
    {
        var configuredPath = Environment.GetEnvironmentVariable("CALIBRE_VIEWER_PATH");
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath)) return configuredPath;

        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                     .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var pathCandidate = Path.Combine(directory.Trim('"'), "ebook-viewer.exe");
            if (File.Exists(pathCandidate)) return pathCandidate;
        }

        foreach (var root in new[]
                 {
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                     Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                 }.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            var candidate = Path.Combine(root, "Calibre2", "ebook-viewer.exe");
            if (File.Exists(candidate)) return candidate;
        }

        throw new Win32Exception("Calibre Ebook Viewer was not found. Install Calibre or set CALIBRE_VIEWER_PATH to ebook-viewer.exe.");
    }
}
