using System.Buffers;

namespace PowerUnit;

public readonly ref struct ArrayPoolItemWrapper<T>
{
    public readonly T[] Buffer { get; }

    public ArrayPoolItemWrapper(int minimumLength)
    {
        Buffer = ArrayPool<T>.Shared.Rent(minimumLength);
    }

    public void Dispose() => ArrayPool<T>.Shared.Return(Buffer);
}

