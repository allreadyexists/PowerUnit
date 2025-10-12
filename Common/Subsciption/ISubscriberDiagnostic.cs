namespace PowerUnit.Common.Subsciption;

public interface ISubscriberDiagnostic
{
    void RcvCounter(string source);
    void ProcessCounter(string source);
    void DropCounter(string source);
}
