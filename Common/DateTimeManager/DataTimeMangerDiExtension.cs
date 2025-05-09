using Microsoft.Extensions.DependencyInjection;

using System.Runtime.InteropServices;

namespace PowerUnit;

public static class DataTimeMangerDiExtension
{
    public static IServiceCollection AddDateTimeManager(this IServiceCollection services)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            services.AddSingleton<IDateTimeManager, WindowsDateTimeManager>();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            services.AddSingleton<IDateTimeManager, LinuxDateTimeManager>();
        else
            throw new NotImplementedException("OSPlatform not support");

        return services;
    }
}

