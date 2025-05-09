namespace PowerUnit;

public sealed record class IEC104ApplicationLayerOption
{
    /// <summary>
    /// Идентификатор сервера
    /// </summary>
    public int ServerId { get; set; }
    /// <summary>
    /// Общий адрес
    /// </summary>
    public ushort CommonASDUAddress { get; set; } = 1;
    /// <summary>
    /// Проверять общий адрес
    /// </summary>
    public bool CheckCommonASDUAddress { get; set; } = true;
    /// <summary>
    /// Разрешена отправка спорадики клиентам
    /// </summary>
    public bool SporadicSendEnabled { get; set; }
    /// <summary>
    /// Размер файловой секции
    /// </summary>
    public uint FileSectionSize { get; set; }
    /// <summary>
    /// Размер файлового сегмента
    /// </summary>
    public uint FileSegmentSize { get; set; }
}

