using System.Runtime.InteropServices;

namespace PowerUnit;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct PacketU
{
    public static byte Size => (byte)Marshal.SizeOf<PacketU>();

    [FieldOffset(0)]
    private readonly byte _u1;
    [FieldOffset(1)]
    private readonly byte _u2 = 0;
    [FieldOffset(2)]
    private readonly byte _u3 = 0;
    [FieldOffset(3)]
    private readonly byte _u4 = 0;

    public PacketU(UControl uControl)
    {
        _u1 = (byte)uControl;
    }

    public bool IsUPacket => (_u1 & 0x3) == 0x3 && _u2 == 0 && _u3 == 0 && _u4 == 0;

    public UControl UControl => (UControl)_u1;
}

