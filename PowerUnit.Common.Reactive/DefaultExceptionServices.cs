using System.Diagnostics.CodeAnalysis;
using System.Reactive.PlatformServices;
using System.Runtime.ExceptionServices;

namespace PowerUnit.Common.Reactive;

internal sealed class DefaultExceptionServices/*Impl*/ : IExceptionServices
{
#if NO_NULLABLE_ATTRIBUTES
#pragma warning disable CS8763 // NB: On down-level platforms, Throw is not marked as DoesNotReturn.
#endif
    [DoesNotReturn]
    public void Rethrow(Exception exception) => ExceptionDispatchInfo.Capture(exception).Throw();
#if NO_NULLABLE_ATTRIBUTES
#pragma warning restore CS8763
#endif
}
