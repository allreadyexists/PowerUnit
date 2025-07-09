using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit.Infrastructure.Db;

public interface IEntityWithDescription
{
    string Description { get; set; }
}

public static class EntityWithDiscriptionHelper
{
    public static void ConfigureDiscription<T>(this EntityTypeBuilder<T> builder, int length = 128) where T : class, IEntityWithDescription
    {
        builder.Property(e => e.Description).HasMaxLength(length).HasDefaultValue(string.Empty);
    }
}
