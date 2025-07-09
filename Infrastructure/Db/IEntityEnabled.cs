using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit.Infrastructure.Db;

public interface IEntityEnabled
{
    bool Enable { get; set; }
}

public static class EntityEnabledHelper
{
    public static void ConfigureEnabled<T>(this EntityTypeBuilder<T> builder) where T : class, IEntityEnabled
    {
        builder.Property(e => e.Enable).HasDefaultValue(true);
    }
}
