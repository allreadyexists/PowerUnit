//namespace PowerUnit;

//internal record struct ServerInfo(IEC104ServerOptions ServerOption, IEC104Server Server)
//{
//    public static implicit operator (IEC104ServerOptions ServerOption, IEC104Server Server)(ServerInfo value) => (value.ServerOption, value.Server);
//    public static implicit operator ServerInfo((IEC104ServerOptions ServerOption, IEC104Server Server) value) => new ServerInfo(value.ServerOption, value.Server);
//}