namespace PowerUnit.Common.Subsciption;

public interface ISubscriberDiagnostic
{
    void RcvCounter(string id, string type);
    void ProcessCounter(string id, string type);
    void DropCounter(string id, string type);
}
