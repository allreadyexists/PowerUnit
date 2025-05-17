namespace PowerUnit;

public abstract record class ServerModel
{
    public string ServerName { get; set; } = string.Empty;
    public int Port { get; set; }
    public KeepAliveModel? KeepAliveOptions { get; set; } = new KeepAliveModel() { KeepAlive = false };
}

