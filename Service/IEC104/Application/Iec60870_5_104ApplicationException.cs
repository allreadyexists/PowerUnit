using PowerUnit.Common.Exceptions;
using PowerUnit.Service.IEC104.Types;

namespace PowerUnit;

internal sealed class IEC60870_5_104ApplicationException : RegularException
{
    public ASDUPacketHeader_2_2 Header { get; private set; }

    public IEC60870_5_104ApplicationException(ASDUPacketHeader_2_2 header, string? message = null) : base(string.IsNullOrEmpty(message) ? header.ToString() : $"{header} : {message}")
    {
        Header = header;
    }
}

