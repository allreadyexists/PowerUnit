using PowerUnit.Infrastructure.Db;

namespace PowerUnit.Infrastructure.IEC104ServerDb;

public class IEC104ServerSqliteDbOptions : DbOptions
{
    public string Database { get; set; } = string.Empty;
}
