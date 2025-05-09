namespace PowerUnit;

public abstract record class ServerOptions
{
    public string ServerName { get; set; } = string.Empty;
    public int Port { get; set; }
    public KeepAliveOptions? KeepAliveOptions { get; set; } = new KeepAliveOptions() { KeepAlive = false };
}

