namespace PowerUnit.Service.IEC104.Types;

public interface IAPCINotification
{
    void NotifyI(in PacketI packetI);
    void NotifyS(in PacketS packetS);
    void NotifyU(in PacketU packetU);
}

