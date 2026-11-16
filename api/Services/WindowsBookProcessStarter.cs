using System.Diagnostics;

namespace DevHelper.Api.Services;

public sealed class WindowsBookProcessStarter : IBookProcessStarter
{
    public void Start(ProcessStartInfo startInfo) => Process.Start(startInfo);
}
