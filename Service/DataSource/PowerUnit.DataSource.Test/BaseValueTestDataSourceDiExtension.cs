using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PowerUnit.Common.Subsciption;
using PowerUnit.Service.IEC104;

namespace PowerUnit.DataSource.Test;

public static class BaseValueTestDataSourceDiExtension
{
    public static IServiceCollection AddBaseValueTestDataSource(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BaseValueTestDataSourceOptions>().Bind(configuration.GetSection(nameof(BaseValueTestDataSourceOptions)));
        services.AddSingleton<IDataSource<BaseValue>, BaseValueTestDataSource>();
        services.AddSingleton<ITestDataSourceDiagnostic, TestDataSourceEmptyDiagnostic>();
        return services;
    }
}
