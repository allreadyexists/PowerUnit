namespace PowerUnit;

[AttributeUsage(AttributeTargets.Struct)]
public class AsduTypeInfoAttribute : Attribute
{
    public AsduType AsduType { get; }
    public SQ SQ { get; }
    public IReadOnlyCollection<COT> ToServerCauseOfTransmits { get; }
    public IReadOnlyCollection<COT> ToClientCauseOfTransmits { get; }

    public AsduTypeInfoAttribute(AsduType asduType, SQ sq, COT[]? toServerCauseOfTransmits = null, COT[]? toClientCauseOfTransmits = null)
    {
        AsduType = asduType;
        SQ = sq;
        ToServerCauseOfTransmits = toServerCauseOfTransmits ?? [];
        ToClientCauseOfTransmits = toClientCauseOfTransmits ?? [];
    }

    public AsduTypeInfoAttribute(AsduType asduType, SQ sq, int[]? toServerCauseOfTransmits = null, int[]? toClientCauseOfTransmits = null)
    {
        AsduType = asduType;
        SQ = sq;
        ToServerCauseOfTransmits = toServerCauseOfTransmits?.Select(x => (COT)x).ToArray() ?? [];
        ToClientCauseOfTransmits = toClientCauseOfTransmits?.Select(x => (COT)x).ToArray() ?? [];
    }
}

