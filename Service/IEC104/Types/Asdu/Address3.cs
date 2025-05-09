using System.Runtime.InteropServices;

namespace PowerUnit.Asdu;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct Address3
{
    public static byte Size => (byte)Marshal.SizeOf<Address3>();

    [FieldOffset(0)]
    internal readonly byte Address0;
    [FieldOffset(1)]
    internal readonly byte Address1;
    [FieldOffset(2)]
    internal readonly byte Address2;

    [FieldOffset(0)]
    public readonly ushort Address;
    [FieldOffset(2)]
    public readonly byte InitAddress;

    public Address3(ushort address, byte initAddress = 0)
    {
        Address = address;
        InitAddress = initAddress;
    }
}

