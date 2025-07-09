using Microsoft.Extensions.DependencyInjection;

using System.Runtime.InteropServices;

namespace PowerUnit.Common.EnviromentManager;

public static class EnviromentManagerDiExtension
{
    public static IEnviromentManager GetEnviromentManager(string serviceName)
    {
        IEnviromentManager enviromentManager;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            enviromentManager = new WindowsEnviromentManager(serviceName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            enviromentManager = new LinuxEnviromentManager(serviceName);
        }
        else
            throw new NotImplementedException("OSPlatform not support");

        return enviromentManager;
    }

    public static IServiceCollection AddEnviromentManager(this IServiceCollection services, string serviceName)
    {
        services.AddSingleton(GetEnviromentManager(serviceName));
        return services;
    }
}
