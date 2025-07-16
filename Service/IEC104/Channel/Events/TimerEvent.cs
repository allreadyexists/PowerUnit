namespace PowerUnit.Service.IEC104.Channel.Events;

/// <summary>
/// Событие срабатывания таймера
/// </summary>
internal sealed class TimerEvent : IEvent
{
    /// <summary>
    /// Идентификатор таймера
    /// </summary>
    public long TimerId { get; }
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="timerId"></param>
    public TimerEvent(long timerId)
    {
        TimerId = timerId;
    }
}

