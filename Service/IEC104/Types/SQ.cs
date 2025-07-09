namespace PowerUnit.Service.IEC104.Types;

public enum SQ : byte
{
#pragma warning disable CA1720 // Identifier contains type name
    Single = 0,
#pragma warning restore CA1720 // Identifier contains type name
    Sequence = 1 << 7
}

