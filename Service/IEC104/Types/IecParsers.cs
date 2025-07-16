using System.Runtime.InteropServices;

namespace PowerUnit.Service.IEC104.Types;

public static class IECParsers
{
    public static void Parse(this IAPCINotification apciNotification, Span<byte> buffer)
    {
        var apci = MemoryMarshal.AsRef<APCI>(buffer[..APCI.Size]);
        if (apci.TryGetIPacket(out var iPacket))
            apciNotification.NotifyI(in iPacket);
        else if (apci.TryGetUPacket(out var uPacket))
            apciNotification.NotifyU(in uPacket);
        else if (apci.TryGetSPacket(out var sPacket))
            apciNotification.NotifyS(in sPacket);
        else
            throw new ArgumentException(nameof(apci));
    }

    public static void ParseHeader(Span<byte> headerBuffer, out ASDUPacketHeader_2_2 header)
    {
        header = MemoryMarshal.AsRef<ASDUPacketHeader_2_2>(headerBuffer);
    }
}

