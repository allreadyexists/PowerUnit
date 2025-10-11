namespace PowerUnit.Common.Shared;

public interface IShared<T> : IDisposable
{
    T AddRef();
}
