namespace PowerUnit.Service.IEC104.Abstract;

public enum ChannelLayerPacketPriority
{
    Low,
    Normal
}

/// <summary>
/// Интерфейс передачи пакета в канальный уровень
/// </summary>
public interface IChannelLayerPacketSender
{
    void Send(byte[] packet, ChannelLayerPacketPriority priority = ChannelLayerPacketPriority.Normal);
}

