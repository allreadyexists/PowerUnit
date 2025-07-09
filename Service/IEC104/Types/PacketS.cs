using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct PacketS
{
    public static byte Size => (byte)Marshal.SizeOf<PacketS>();

    [FieldOffset(0)]
    private readonly byte _s1 = 1;
    [FieldOffset(1)]
    private readonly byte _s2 = 0;
    [FieldOffset(2)]
    private readonly byte _s3;
    [FieldOffset(3)]
    private readonly byte _s4;

    [FieldOffset(2)]
    private readonly ushort _rx;

    public PacketS(ushort rx)
    {
        //if (rx > 0x7FFF)
        //    throw new ArgumentOutOfRangeException();
        _rx = (ushort)(rx << 1);
    }

    public bool IsSPacket => _s1 == 1 && _s2 == 0 && (_s3 & 1) == 0;

    public ushort Rx => (ushort)(_rx >> 1);
}

