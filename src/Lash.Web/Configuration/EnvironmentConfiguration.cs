using DotNetEnv;

namespace Lash.Web.Configuration;

public static class EnvironmentConfiguration
{
    public static void Load()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var environmentFile = Path.Combine(currentDirectory, ".env");
        if (!File.Exists(environmentFile))
        {
            environmentFile = Path.Combine(currentDirectory, "src", "Lash.Web", ".env");
        }

        if (File.Exists(environmentFile))
        {
            Env.NoClobber().Load(environmentFile);
        }
    }
}
