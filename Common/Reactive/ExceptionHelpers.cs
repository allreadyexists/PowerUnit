using System.Diagnostics.CodeAnalysis;
using System.Reactive.PlatformServices;

namespace PowerUnit.Common.Reactive;

internal static class ExceptionHelpers
{
    private static readonly Lazy<IExceptionServices> Services = new(Initialize);

    [DoesNotReturn]
    public static void Throw(this Exception exception) => Services.Value.Rethrow(exception);

    private static IExceptionServices Initialize()
    {
        return PlatformEnlightenmentProvider.Current.GetService<IExceptionServices>() ?? new DefaultExceptionServices();
    }
}
