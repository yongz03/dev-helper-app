using System.Diagnostics;

namespace DevHelper.Api.Services;

public interface IBookProcessStarter
{
    void Start(ProcessStartInfo startInfo);
}
