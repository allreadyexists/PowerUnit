using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct FileLength
{
    public static byte Size => (byte)Marshal.SizeOf<FileLength>();

    [FieldOffset(0)]
    private readonly byte _length0;
    [FieldOffset(1)]
    private readonly byte _length1;
    [FieldOffset(2)]
    private readonly byte _length2;

    public FileLength(uint length)
    {
        //if (length > 0xFFFFFF)
        //    throw new ArgumentException(nameof(length));
        _length0 = (byte)(length >> 0 & 0xFF);
        _length1 = (byte)(length >> 8 & 0xFF);
        _length2 = (byte)(length >> 16 & 0xFF);
    }

    public uint Value => (uint)(_length2 << 16 | _length1 << 8 | _length0);
}
