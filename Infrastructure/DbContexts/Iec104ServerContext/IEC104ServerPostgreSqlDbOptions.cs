using PowerUnit.Infrastructure.Db;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104ServerPostgreSqlDbOptions : DbOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 6543;
    public string Database { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
