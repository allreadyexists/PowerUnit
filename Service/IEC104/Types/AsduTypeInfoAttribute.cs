namespace PowerUnit.Service.IEC104.Types;

[AttributeUsage(AttributeTargets.Struct)]
public class ASDUTypeInfoAttribute : Attribute
{
    public ASDUType AsduType { get; }
    public SQ SQ { get; }
    public IReadOnlyCollection<COT> ToServerCauseOfTransmits { get; }
    public IReadOnlyCollection<COT> ToClientCauseOfTransmits { get; }

    public ASDUTypeInfoAttribute(ASDUType asduType, SQ sq, COT[]? toServerCauseOfTransmits = null, COT[]? toClientCauseOfTransmits = null)
    {
        AsduType = asduType;
        SQ = sq;
        ToServerCauseOfTransmits = toServerCauseOfTransmits ?? [];
        ToClientCauseOfTransmits = toClientCauseOfTransmits ?? [];
    }

    public ASDUTypeInfoAttribute(ASDUType asduType, SQ sq, int[]? toServerCauseOfTransmits = null, int[]? toClientCauseOfTransmits = null)
    {
        AsduType = asduType;
        SQ = sq;
        ToServerCauseOfTransmits = toServerCauseOfTransmits?.Select(x => (COT)x).ToArray() ?? [];
        ToClientCauseOfTransmits = toClientCauseOfTransmits?.Select(x => (COT)x).ToArray() ?? [];
    }
}

