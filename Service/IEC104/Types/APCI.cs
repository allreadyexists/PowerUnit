using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct APCI
{
    public const byte START_PACKET = 0x68;
    public const byte MIN_LENGTH = 4;
    public static byte Size => (byte)Marshal.SizeOf<APCI>();

    [FieldOffset(0)]
    public readonly byte StartPacket = START_PACKET;
    [FieldOffset(1)]
    public readonly byte Length;
    [FieldOffset(2)]
    internal readonly PacketI IPacket;
    [FieldOffset(2)]
    internal readonly PacketU UPacket;
    [FieldOffset(2)]
    internal readonly PacketS SPacket;

    public APCI(byte length, PacketI iPacket)
    {
        Length = length;
        IPacket = iPacket;
    }

    public APCI(byte length, PacketU uPacket)
    {
        Length = length;
        UPacket = uPacket;
    }

    public APCI(byte length, PacketS sPacket)
    {
        Length = length;
        SPacket = sPacket;
    }

    public bool TryGetIPacket(out PacketI iPacket)
    {
        iPacket = IPacket;
        return IPacket.IsIPacket;
    }

    public bool TryGetSPacket(out PacketS sPacket)
    {
        sPacket = SPacket;
        return SPacket.IsSPacket;
    }

    public bool TryGetUPacket(out PacketU uPacket)
    {
        uPacket = UPacket;
        return UPacket.IsUPacket;
    }
}

