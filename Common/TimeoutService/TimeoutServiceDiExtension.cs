using Microsoft.Extensions.DependencyInjection;

namespace PowerUnit.Common.TimeoutService;

public static class TimeoutServiceDiExtension
{
    public static IServiceCollection AddTimeoutService(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Singleton)
    {
        switch (lifeTime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<ITimeoutService, TimeoutService>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<ITimeoutService, TimeoutService>();
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped<ITimeoutService, TimeoutService>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifeTime), lifeTime, null);
        }

        return services;
    }
}
