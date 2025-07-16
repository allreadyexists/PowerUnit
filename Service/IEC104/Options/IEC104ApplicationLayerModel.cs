namespace PowerUnit.Service.IEC104;

public sealed record class IEC104ApplicationLayerModel
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
}

