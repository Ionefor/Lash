using Xunit;

namespace Lash.Web.IntegrationTests;

public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!IsDockerConfigured())
        {
            Skip = "Docker is required for this container integration test.";
        }
    }

    private static bool IsDockerConfigured()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DOCKER_HOST")))
        {
            return true;
        }

        return OperatingSystem.IsWindows()
            ? File.Exists("\\\\.\\pipe\\docker_engine")
            : File.Exists("/var/run/docker.sock");
    }
}
