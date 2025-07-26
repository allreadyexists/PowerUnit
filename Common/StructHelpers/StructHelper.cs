using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PowerUnit.Common.StructHelpers;

public static class StructHelper
{
    public static void Serialize<T>(this ref T s, byte[] array, int offset)
        where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(s, ptr, true);
            Marshal.Copy(ptr, array, offset, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public static T Deserialize<T>(this byte[] array, int offset)
        where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var ptr = Marshal.AllocHGlobal(size);
        T s = default;
        try
        {
            Marshal.Copy(array, offset, ptr, size);
            s = Marshal.PtrToStructure<T>(ptr)!;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return s;
    }

    public static unsafe void SerializeUnsafe<T>(this T s, byte[] array, int offset)
        where T : unmanaged
    {
        s.SerializeUnsafe(array.AsSpan(), offset);
    }

    public static unsafe void SerializeUnsafe<T>(this T s, Span<byte> array, int offset)
        where T : unmanaged
    {
        if (offset + Marshal.SizeOf(s) > array.Length)
            throw new ArgumentOutOfRangeException(nameof(array));
        fixed (byte* bufferPtr = array)
        {
            Buffer.MemoryCopy(&s, bufferPtr + offset, sizeof(T), sizeof(T));
        }
    }

    public static unsafe T DeserializeUnsafe<T>(this byte[] array, int offset)
        where T : unmanaged
    {
        T result = default;

        if (offset + Marshal.SizeOf(result) > array.Length)
            throw new ArgumentOutOfRangeException(nameof(array));

        fixed (byte* bufferPtr = array)
        {
            Buffer.MemoryCopy(bufferPtr + offset, &result, sizeof(T), sizeof(T));
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> AsSpan<T>(this ref T val) where T : unmanaged => MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref val, 1));
}

