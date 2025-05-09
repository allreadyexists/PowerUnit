namespace PowerUnit.Models;

public abstract record class ServerModel
{
    public string ServerName { get; set; } = string.Empty;
    public int Port { get; set; } = 2404;
    public KeepAliveOptions? KeepAliveOptions { get; set; } = new KeepAliveOptions() { KeepAlive = false };
}

