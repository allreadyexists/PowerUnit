using PowerUnit.Service.IEC104.Types;

namespace PowerUnit;

internal sealed class Iec60870_5_104ApplicationException : RegularException
{
    public AsduPacketHeader_2_2 Header { get; private set; }

    public Iec60870_5_104ApplicationException(AsduPacketHeader_2_2 header, string? message = null) : base(string.IsNullOrEmpty(message) ? header.ToString() : $"{header} : {message}")
    {
        Header = header;
    }
}

