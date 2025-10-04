namespace PowerUnit.Common.TimeoutService;

public interface ITimeoutOwner
{
    void NotifyTimeoutReady(long timeout);
}
