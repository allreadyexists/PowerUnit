namespace PowerUnit;

/// <summary>
/// Получение уведомлений от физического канала связи
/// </summary>
public interface IPhysicalLayerNotification
{
    /// <summary>
    /// Установлено соединение
    /// </summary>
    void Connect();
    /// <summary>
    /// Разорвано соединение
    /// </summary>
    void Disconnect();
    /// <summary>
    /// Принят пакет
    /// </summary>
    /// <param name="packet"></param>
    void Recieve(byte[] packet);
}

