namespace PowerUnit.Common.TimeoutService;

public interface ITimeoutServiceDiagnostic
{
    void TimerCallbackDuration(double duration);
    void TimerCallbackCall();
    void CreateTimeoutDuration(double duration);
    void CreateTimeoutCall();
    void RestartTimeoutDuration(double duration);
    void RestartTimeoutCall();
    void CancelTimeoutDuration(double duration);
    void CancelTimeoutCall();
}

internal sealed class TimeoutServiceDiagnosticIdle : ITimeoutServiceDiagnostic
{
    void ITimeoutServiceDiagnostic.CancelTimeoutCall() { }
    void ITimeoutServiceDiagnostic.CancelTimeoutDuration(double duration) { }
    void ITimeoutServiceDiagnostic.CreateTimeoutCall() { }
    void ITimeoutServiceDiagnostic.CreateTimeoutDuration(double duration) { }
    void ITimeoutServiceDiagnostic.RestartTimeoutCall() { }
    void ITimeoutServiceDiagnostic.RestartTimeoutDuration(double duration) { }
    void ITimeoutServiceDiagnostic.TimerCallbackCall() { }
    void ITimeoutServiceDiagnostic.TimerCallbackDuration(double duration) { }
}
