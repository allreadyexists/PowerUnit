namespace PowerUnit.DataSource.Test;

internal enum GenerateMode
{
    Single,
    Random,
    Block
}

internal sealed class BaseValueTestDataSourceOptions
{
    public GenerateMode GenerateMode { get; set; } = GenerateMode.Block;
    public uint Rps { get; set; } = 50_000;
}
