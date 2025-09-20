using System.Reactive.Disposables;

namespace PowerUnit.Common.Reactive;

internal sealed class BooleanDisposable2 : ICancelable
{
    // Keep internal! This is used as sentinel in other IDisposable implementations to detect disposal and
    // should never be exposed to user code in order to prevent users from swapping in the sentinel. Have
    // a look at the code in e.g. SingleAssignmentDisposable for usage patterns.
    internal static readonly BooleanDisposable2 True = new(true);

    private volatile bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanDisposable"/> class.
    /// </summary>
    public BooleanDisposable2()
    {
    }

    private BooleanDisposable2(bool isDisposed)
    {
        _isDisposed = isDisposed;
    }

    /// <summary>
    /// Gets a value that indicates whether the object is disposed.
    /// </summary>
    public bool IsDisposed => _isDisposed;

    /// <summary>
    /// Sets the status to disposed, which can be observer through the <see cref="IsDisposed"/> property.
    /// </summary>
    public void Dispose()
    {
        _isDisposed = true;
    }
}
