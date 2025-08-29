namespace PowerUnit.Common.Options;

public record class KeepAliveModel
{
    public bool KeepAlive { get; set; }
    public int TcpKeepAliveTime { get; set; } = 600;
    public int TcpKeepAliveInterval { get; set; } = 60;
}

