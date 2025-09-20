using System.Reactive.Disposables;

namespace PowerUnit.Common.Reactive;

internal sealed class AnonymousDisposable : ICancelable
{
    private volatile Action? _dispose;

    /// <summary>
    /// Constructs a new disposable with the given action used for disposal.
    /// </summary>
    /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
    public AnonymousDisposable(Action dispose)
    {
        //Diagnostics.Debug.Assert(dispose != null);

        _dispose = dispose;
    }

    /// <summary>
    /// Gets a value that indicates whether the object is disposed.
    /// </summary>
    public bool IsDisposed => _dispose == null;

    /// <summary>
    /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
    /// </summary>
    public void Dispose()
    {
        Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}
