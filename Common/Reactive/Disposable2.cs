using System.Diagnostics.CodeAnalysis;

namespace PowerUnit.Common.Reactive;

public static partial class Disposable2
{
    /// <summary>
    /// Represents a disposable that does nothing on disposal.
    /// </summary>
    private sealed class EmptyDisposable : IDisposable
    {
        /// <summary>
        /// Singleton default disposable.
        /// </summary>
        public static readonly EmptyDisposable Instance = new();

        private EmptyDisposable()
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Dispose()
        {
            // no op
        }
    }

    /// <summary>
    /// Gets the disposable that does nothing when disposed.
    /// </summary>
    public static IDisposable Empty => EmptyDisposable.Instance;

    /// <summary>
    /// Creates a disposable object that invokes the specified action when disposed.
    /// </summary>
    /// <param name="dispose">Action to run during the first call to <see cref="IDisposable.Dispose"/>. The action is guaranteed to be run at most once.</param>
    /// <returns>The disposable object that runs the given action upon disposal.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dispose"/> is <c>null</c>.</exception>
    public static IDisposable Create(Action dispose)
    {
        ArgumentNullException.ThrowIfNull(dispose);

        return new AnonymousDisposable(dispose);
    }

    /// <summary>
    /// Creates a disposable object that invokes the specified action when disposed.
    /// </summary>
    /// <param name="state">The state to be passed to the action.</param>
    /// <param name="dispose">Action to run during the first call to <see cref="IDisposable.Dispose"/>. The action is guaranteed to be run at most once.</param>
    /// <returns>The disposable object that runs the given action upon disposal.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dispose"/> is <c>null</c>.</exception>
    public static IDisposable Create<TState>(TState state, Action<TState> dispose)
    {
        ArgumentNullException.ThrowIfNull(dispose);

        return new AnonymousDisposable<TState>(state, dispose);
    }

    /// <summary>
    /// Gets the value stored in <paramref name="fieldRef" /> or a null if
    /// <paramref name="fieldRef" /> was already disposed.
    /// </summary>
    internal static IDisposable? GetValue([NotNullIfNotNull(nameof(fieldRef))] /*in*/ ref IDisposable? fieldRef)
    {
        var current = Volatile.Read(ref fieldRef);

        return current == BooleanDisposable2.True
            ? null
            : current;
    }

    /// <summary>
    /// Gets the value stored in <paramref name="fieldRef" /> or a no-op-Disposable if
    /// <paramref name="fieldRef" /> was already disposed.
    /// </summary>
    [return: NotNullIfNotNull(nameof(fieldRef))]
    internal static IDisposable? GetValueOrDefault([NotNullIfNotNull(nameof(fieldRef))] /*in*/ ref IDisposable? fieldRef)
    {
        var current = Volatile.Read(ref fieldRef);

        return current == BooleanDisposable2.True
            ? EmptyDisposable.Instance
            : current;
    }

    /// <summary>
    /// Tries to assign <paramref name="value" /> to <paramref name="fieldRef" />.
    /// </summary>
    /// <returns>A <see cref="TrySetSingleResult2"/> value indicating the outcome of the operation.</returns>
    internal static TrySetSingleResult2 TrySetSingle([NotNullIfNotNull(nameof(value))] ref IDisposable? fieldRef, IDisposable? value)
    {
        var old = Interlocked.CompareExchange(ref fieldRef, value, null);
        if (old == null)
        {
            return TrySetSingleResult2.Success;
        }

        if (old != BooleanDisposable2.True)
        {
            return TrySetSingleResult2.AlreadyAssigned;
        }

        value?.Dispose();
        return TrySetSingleResult2.Disposed;
    }

    /// <summary>
    /// Tries to assign <paramref name="value" /> to <paramref name="fieldRef" />. If <paramref name="fieldRef" />
    /// is not disposed and is assigned a different value, it will not be disposed.
    /// </summary>
    /// <returns>true if <paramref name="value" /> was successfully assigned to <paramref name="fieldRef" />.</returns>
    /// <returns>false <paramref name="fieldRef" /> has been disposed.</returns>
    internal static bool TrySetMultiple([NotNullIfNotNull(nameof(value))] ref IDisposable? fieldRef, IDisposable? value)
    {
        // Let's read the current value atomically (also prevents reordering).
        var old = Volatile.Read(ref fieldRef);

        for (; ; )
        {
            // If it is the disposed instance, dispose the value.
            if (old == BooleanDisposable2.True)
            {
                value?.Dispose();
                return false;
            }

            // Atomically swap in the new value and get back the old.
            var b = Interlocked.CompareExchange(ref fieldRef, value, old);

            // If the old and new are the same, the swap was successful and we can quit
            if (old == b)
            {
                return true;
            }

            // Otherwise, make the old reference the current and retry.
            old = b;
        }
    }

    /// <summary>
    /// Tries to assign <paramref name="value" /> to <paramref name="fieldRef" />. If <paramref name="fieldRef" />
    /// is not disposed and is assigned a different value, it will be disposed.
    /// </summary>
    /// <returns>true if <paramref name="value" /> was successfully assigned to <paramref name="fieldRef" />.</returns>
    /// <returns>false <paramref name="fieldRef" /> has been disposed.</returns>
    internal static bool TrySetSerial([NotNullIfNotNull(nameof(value))] ref IDisposable? fieldRef, IDisposable? value)
    {
        var copy = Volatile.Read(ref fieldRef);
        for (; ; )
        {
            if (copy == BooleanDisposable2.True)
            {
                value?.Dispose();
                return false;
            }

            var current = Interlocked.CompareExchange(ref fieldRef, value, copy);
            if (current == copy)
            {
                copy?.Dispose();
                return true;
            }

            copy = current;
        }
    }

    /// <summary>
    /// Disposes <paramref name="fieldRef" />. 
    /// </summary>
    internal static void Dispose([NotNullIfNotNull(nameof(fieldRef))] ref IDisposable? fieldRef)
    {
        var old = Interlocked.Exchange(ref fieldRef, BooleanDisposable2.True);

        if (old != BooleanDisposable2.True)
        {
            old?.Dispose();
        }
    }
}
