namespace K2.Common.Prometheus;

public class PrometheusConfig
{
    public const string CONFIG_SECTION_NAME = "Prometheus";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseKestrel { get; set; }
}
