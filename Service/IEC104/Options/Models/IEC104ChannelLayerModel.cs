namespace PowerUnit;

public sealed record class IEC104ChannelLayerModel
{
    /// <summary>
    /// Таймаут при установки соединения
    /// </summary>
    public byte Timeout0Sec { get; set; } = 30;
    /// <summary>
    /// Таймаут при посылке или тестировании APDU
    /// </summary>
    public byte Timeout1Sec { get; set; } = 15;
    /// <summary>
    /// Таймаут для подтверждения в случае отсутствия сообщения с данными Timeout2Sec < Timeout1Sec
    /// </summary>
    public byte Timeout2Sec { get; set; } = 10;
    /// <summary>
    /// Таймаут для посылки блоков тестирования в случае долгого простоя
    /// </summary>
    public byte Timeout3Sec { get; set; } = 20;

    /// <summary>
    /// Максимальная разность между переменной состояния передачи и номером последнего подтвержденного APDU
    /// </summary>
    public ushort WindowKSize { get; set; } = 12;
    /// <summary>
    ///  Последнее подтверждение после приема W APDU формата I. Рекомендуется <= 2/3 * WindowKSize
    /// </summary>
    public ushort WindowWSize { get; set; } = 8;

    /// <summary>
    /// Использовать фрагментированную отправка ответов
    /// </summary>
    public bool UseFragmentSend { get; set; }
}

