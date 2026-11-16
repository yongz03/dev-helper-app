using System.Diagnostics;
using DevHelper.Api.Services;
using Xunit;

namespace DevHelper.Api.Tests;

public sealed class BookLauncherTests
{
    private sealed class RecordingProcessStarter : IBookProcessStarter
    {
        public ProcessStartInfo? StartInfo { get; private set; }
        public void Start(ProcessStartInfo startInfo) => StartInfo = startInfo;
    }

    [Fact]
    public void Open_Epub_LaunchesCalibreViewerWithTheBookPath()
    {
        var bookPath = Path.Combine(Path.GetTempPath(), $"dev-helper-book-{Guid.NewGuid():N}.epub");
        var viewerPath = Path.Combine(Path.GetTempPath(), $"ebook-viewer-{Guid.NewGuid():N}.exe");
        File.WriteAllText(bookPath, "epub content");
        File.WriteAllText(viewerPath, "viewer placeholder");
        var previousViewerPath = Environment.GetEnvironmentVariable("CALIBRE_VIEWER_PATH");

        try
        {
            Environment.SetEnvironmentVariable("CALIBRE_VIEWER_PATH", viewerPath);
            var processStarter = new RecordingProcessStarter();
            var launcher = new BookLauncher(processStarter);

            launcher.Open(bookPath);

            Assert.NotNull(processStarter.StartInfo);
            Assert.Equal(viewerPath, processStarter.StartInfo.FileName);
            Assert.Equal(bookPath, Assert.Single(processStarter.StartInfo.ArgumentList));
            Assert.False(processStarter.StartInfo.UseShellExecute);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CALIBRE_VIEWER_PATH", previousViewerPath);
            File.Delete(bookPath);
            File.Delete(viewerPath);
        }
    }
}
