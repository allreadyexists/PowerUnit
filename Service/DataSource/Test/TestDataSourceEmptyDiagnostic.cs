namespace PowerUnit.DataSource.Test;

internal sealed class TestDataSourceEmptyDiagnostic : ITestDataSourceDiagnostic
{
    void ITestDataSourceDiagnostic.BatchGenerateDuration(long durationUs) { }

    void ITestDataSourceDiagnostic.IncRequest() { }
}
