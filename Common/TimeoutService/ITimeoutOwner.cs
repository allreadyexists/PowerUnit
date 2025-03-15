namespace PowerUnit.Common.TimeoutService;

public interface ITimeoutOwner
{
    Task NotifyTimeoutReadyAsync(long timeout, CancellationToken cancellationToken);
}
