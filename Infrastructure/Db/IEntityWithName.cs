using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit.Infrastructure.Db;

public interface IEntityWithName
{
    string Name { get; set; }
}

public static class EntityWithNameHelper
{
    public static void ConfigureName<T>(this EntityTypeBuilder<T> builder, int length = 64) where T : class, IEntityWithName
    {
        builder.Property(e => e.Name).HasMaxLength(length).IsRequired().HasDefaultValue(string.Empty);
    }
}
