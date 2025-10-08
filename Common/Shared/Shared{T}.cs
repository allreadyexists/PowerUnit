using Microsoft.Extensions.ObjectPool;

namespace PowerUnit.Common.Shared;

public struct Shared<T> : IDisposable where T : class
{
    private static readonly ObjectPool<SharedContainer<T>> _pool = new DefaultObjectPoolProvider().Create<SharedContainer<T>>();
    private readonly SharedContainer<T> _sharedContainer;

    public readonly T Value => _sharedContainer.Value!;
    private int _dispose;

    public Shared(Func<object?, T> initValue, Action<T>? freeValue = null, object? ctx = null)
    {
        _sharedContainer = _pool.Get();
        _sharedContainer.RefCount = 1;
        _sharedContainer.Value = initValue(ctx);
        _sharedContainer.Free = freeValue;
    }

    public readonly Shared<T> AddRef()
    {
        Interlocked.Increment(ref _sharedContainer.RefCount);
        return this;
    }

    void IDisposable.Dispose()
    {
        if (Interlocked.Exchange(ref _dispose, 1) != 0)
            return;

        var refCount = Interlocked.Decrement(ref _sharedContainer.RefCount);
        if (refCount == 0)
        {
            _sharedContainer.Free?.Invoke(_sharedContainer.Value!);
            _sharedContainer.Value = null;
            _pool.Return(_sharedContainer);
        }
    }

    private class SharedContainer<TT> where TT : class
    {
        public TT? Value;
        public Action<TT>? Free;
        public int RefCount;
    }
}
