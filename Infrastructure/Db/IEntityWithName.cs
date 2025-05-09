using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit;

internal interface IEntityWithName
{
    public string Name { get; set; }
}

internal static class EntityWithNameHelper
{
    public static void ConfigureName<T>(this EntityTypeBuilder<T> builder, int length = 64) where T : class, IEntityWithName
    {
        builder.Property(e => e.Name).HasMaxLength(length).IsRequired().HasDefaultValue(string.Empty);
    }
}
