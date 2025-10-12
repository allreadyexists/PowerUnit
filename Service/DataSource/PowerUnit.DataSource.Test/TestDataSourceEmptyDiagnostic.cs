namespace PowerUnit.DataSource.Test;

internal sealed class TestDataSourceEmptyDiagnostic : ITestDataSourceDiagnostic
{
    void ITestDataSourceDiagnostic.IncRequest() { }
}
