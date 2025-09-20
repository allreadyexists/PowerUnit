using System.Reactive;
using System.Reactive.Disposables;

namespace PowerUnit.Common.Reactive;

internal abstract class SafeObserver<TSource> : ISafeObserver<TSource>
{
    private sealed class WrappingSafeObserver : SafeObserver<TSource>
    {
        private readonly IObserver<TSource> _observer;

        public WrappingSafeObserver(IObserver<TSource> observer)
        {
            _observer = observer;
        }

        public override void OnNext(TSource value)
        {
            var noError = false;
            try
            {
                _observer.OnNext(value);
                noError = true;
            }
            finally
            {
                if (!noError)
                {
                    Dispose();
                }
            }
        }

        public override void OnError(Exception error)
        {
            using (this)
            {
                _observer.OnError(error);
            }
        }

        public override void OnCompleted()
        {
            using (this)
            {
                _observer.OnCompleted();
            }
        }
    }

    public static ISafeObserver<TSource> Wrap(IObserver<TSource> observer)
    {
        if (observer is AnonymousObserver<TSource> a)
        {
            return a.MakeSafe();
        }

        return new WrappingSafeObserver(observer);
    }

    private SingleAssignmentDisposableValue _disposable;

    public abstract void OnNext(TSource value);

    public abstract void OnError(Exception error);

    public abstract void OnCompleted();

    public void SetResource(IDisposable resource)
    {
        _disposable.Disposable = resource;
    }

    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposable.Dispose();
        }
    }

    /// <summary>
    /// Class to create an <see cref="IObserver{T}"/> instance from delegate-based implementations of the On* methods.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    public sealed class AnonymousObserver<T> : ObserverBase<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        /// <summary>
        /// Creates an observer from the specified <see cref="IObserver{T}.OnNext(T)"/>, <see cref="IObserver{T}.OnError(Exception)"/>, and <see cref="IObserver{T}.OnCompleted()"/> actions.
        /// </summary>
        /// <param name="onNext">Observer's <see cref="IObserver{T}.OnNext(T)"/> action implementation.</param>
        /// <param name="onError">Observer's <see cref="IObserver{T}.OnError(Exception)"/> action implementation.</param>
        /// <param name="onCompleted">Observer's <see cref="IObserver{T}.OnCompleted()"/> action implementation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="onNext"/> or <paramref name="onError"/> or <paramref name="onCompleted"/> is <c>null</c>.</exception>
        public AnonymousObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            _onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
            _onError = onError ?? throw new ArgumentNullException(nameof(onError));
            _onCompleted = onCompleted ?? throw new ArgumentNullException(nameof(onCompleted));
        }

        /// <summary>
        /// Creates an observer from the specified <see cref="IObserver{T}.OnNext(T)"/> action.
        /// </summary>
        /// <param name="onNext">Observer's <see cref="IObserver{T}.OnNext(T)"/> action implementation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="onNext"/> is <c>null</c>.</exception>
        public AnonymousObserver(Action<T> onNext)
            : this(onNext, Stubs.Throw, Stubs.Nop)
        {
        }

        /// <summary>
        /// Creates an observer from the specified <see cref="IObserver{T}.OnNext(T)"/> and <see cref="IObserver{T}.OnError(Exception)"/> actions.
        /// </summary>
        /// <param name="onNext">Observer's <see cref="IObserver{T}.OnNext(T)"/> action implementation.</param>
        /// <param name="onError">Observer's <see cref="IObserver{T}.OnError(Exception)"/> action implementation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="onNext"/> or <paramref name="onError"/> is <c>null</c>.</exception>
        public AnonymousObserver(Action<T> onNext, Action<Exception> onError)
            : this(onNext, onError, Stubs.Nop)
        {
        }

        /// <summary>
        /// Creates an observer from the specified <see cref="IObserver{T}.OnNext(T)"/> and <see cref="IObserver{T}.OnCompleted()"/> actions.
        /// </summary>
        /// <param name="onNext">Observer's <see cref="IObserver{T}.OnNext(T)"/> action implementation.</param>
        /// <param name="onCompleted">Observer's <see cref="IObserver{T}.OnCompleted()"/> action implementation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="onNext"/> or <paramref name="onCompleted"/> is <c>null</c>.</exception>
        public AnonymousObserver(Action<T> onNext, Action onCompleted)
            : this(onNext, Stubs.Throw, onCompleted)
        {
        }

        /// <summary>
        /// Calls the action implementing <see cref="IObserver{T}.OnNext(T)"/>.
        /// </summary>
        /// <param name="value">Next element in the sequence.</param>
        protected override void OnNextCore(T value) => _onNext(value);

        /// <summary>
        /// Calls the action implementing <see cref="IObserver{T}.OnError(Exception)"/>.
        /// </summary>
        /// <param name="error">The error that has occurred.</param>
        protected override void OnErrorCore(Exception error) => _onError(error);

        /// <summary>
        /// Calls the action implementing <see cref="IObserver{T}.OnCompleted()"/>.
        /// </summary>
        protected override void OnCompletedCore() => _onCompleted();

        internal ISafeObserver<T> MakeSafe() => new AnonymousSafeObserver<T>(_onNext, _onError, _onCompleted);
    }

    /// <summary>
    /// This class fuses logic from ObserverBase, AnonymousObserver, and SafeObserver into one class. When an observer
    /// needs to be safeguarded, an instance of this type can be created by SafeObserver.Create when it detects its
    /// input is an AnonymousObserver, which is commonly used by end users when using the Subscribe extension methods
    /// that accept delegates for the On* handlers. By doing the fusion, we make the call stack depth shorter which
    /// helps debugging and some performance.
    /// </summary>
    internal sealed class AnonymousSafeObserver<T> : SafeObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        private int _isStopped;

        public AnonymousSafeObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public override void OnNext(T value)
        {
            if (_isStopped == 0)
            {
                var noError = false;
                try
                {
                    _onNext(value);
                    noError = true;
                }
                finally
                {
                    if (!noError)
                    {
                        Dispose();
                    }
                }
            }
        }

        public override void OnError(Exception error)
        {
            if (Interlocked.Exchange(ref _isStopped, 1) == 0)
            {
                using (this)
                {
                    _onError(error);
                }
            }
        }

        public override void OnCompleted()
        {
            if (Interlocked.Exchange(ref _isStopped, 1) == 0)
            {
                using (this)
                {
                    _onCompleted();
                }
            }
        }
    }

    internal static class Stubs
    {
        public static readonly Action Nop = static () => { };
        public static readonly Action<Exception> Throw = static ex => { ex.Throw(); };
    }
}
