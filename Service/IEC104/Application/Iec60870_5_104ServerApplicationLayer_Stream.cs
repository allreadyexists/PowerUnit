using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104.Application;

public sealed partial class IEC60870_5_104ServerApplicationLayer
{
    private void Stream(byte[] buffer, IList<MapValueItem> values)
    {
        SendValues(buffer, 0, COT.SPORADIC, values);
    }
}

