using System.Reactive.Disposables;

namespace PowerUnit.Common.Reactive;

/// <summary>
/// Represents a disposable resource whose underlying disposable resource can be replaced by another disposable resource, causing automatic disposal of the previous underlying disposable resource.
/// </summary>
internal struct SerialDisposableValue : ICancelable
{
    private IDisposable? _current;

    /// <summary>
    /// Gets a value that indicates whether the object is disposed.
    /// </summary>
    public bool IsDisposed =>
        // We use a sentinel value to indicate we've been disposed. This sentinel never leaks
        // to the outside world (see the Disposable property getter), so no-one can ever assign
        // this value to us manually.
        Volatile.Read(ref _current) == BooleanDisposable2.True;

    /// <summary>
    /// Gets or sets the underlying disposable.
    /// </summary>
    /// <remarks>If the SerialDisposable has already been disposed, assignment to this property causes immediate disposal of the given disposable object. Assigning this property disposes the previous disposable object.</remarks>
    public IDisposable? Disposable
    {
        get => /*System.Reactive.Disposables.*/Disposable2.GetValue(ref _current);
        set => /*System.Reactive.Disposables.*/Disposable2.TrySetSerial(ref _current, value);
    }

    public bool TrySetFirst(IDisposable disposable) => /*Disposables.*/Disposable2.TrySetSingle(ref _current, disposable) == TrySetSingleResult2.Success;

    /// <summary>
    /// Disposes the underlying disposable as well as all future replacements.
    /// </summary>
    public void Dispose()
    {
        /*Disposables.*/
        Disposable2.Dispose(ref _current);
    }
}
