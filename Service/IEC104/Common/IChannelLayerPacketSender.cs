namespace PowerUnit;

/// <summary>
/// Интерфейс передачи пакета в канальный уровень
/// </summary>
public interface IChannelLayerPacketSender
{
    void Send(byte[] packet);
}

