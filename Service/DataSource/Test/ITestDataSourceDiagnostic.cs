namespace PowerUnit.DataSource.Test;

public interface ITestDataSourceDiagnostic
{
    void IncRequest();
    void BatchGenerateDuration(long durationUs);
}
