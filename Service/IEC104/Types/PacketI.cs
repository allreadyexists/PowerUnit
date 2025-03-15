using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct PacketI
{
    public static byte Size => (byte)Marshal.SizeOf<PacketI>();

    [FieldOffset(0)]
    private readonly byte _i1;
    [FieldOffset(1)]
    private readonly byte _i2;
    [FieldOffset(2)]
    private readonly byte _i3;
    [FieldOffset(3)]
    private readonly byte _i4;

    [FieldOffset(0)]
    private readonly ushort _tx;
    [FieldOffset(2)]
    private readonly ushort _rx;

    public PacketI(ushort tx, ushort rx)
    {
        _tx = (ushort)(tx << 1);
        _rx = (ushort)(rx << 1);
    }

    public bool IsIPacket => (_i1 & 1) == 0 && (_i3 & 1) == 0;

    public ushort Tx => (ushort)(_tx >> 1);

    public ushort Rx => (ushort)(_rx >> 1);
}

