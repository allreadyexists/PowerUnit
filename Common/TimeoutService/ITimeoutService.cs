namespace PowerUnit;

public interface ITimeoutService
{
    /// <summary>
    /// Return timeout Id
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> CreateTimeoutAsync(ITimeoutOwner owner, TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Restart timeout
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="timeoutId"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RestartTimeoutAsync(ITimeoutOwner owner, long timeoutId, TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Remove timeout
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="timeoutId"></param>
    /// <param name="cancellationToken"></param>
    Task CancelTimeoutAsync(ITimeoutOwner owner, long timeoutId, CancellationToken cancellationToken);
}
