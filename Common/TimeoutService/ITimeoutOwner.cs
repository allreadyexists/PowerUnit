namespace PowerUnit;

public interface ITimeoutOwner
{
    Task NotifyTimeoutReadyAsync(long timeout, CancellationToken cancellationToken);
}
