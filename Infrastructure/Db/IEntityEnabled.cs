using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerUnit;

internal interface IEntityEnabled
{
    public bool Enable { get; set; }
}

internal static class EntityEnabledHelper
{
    public static void ConfigureEnabled<T>(this EntityTypeBuilder<T> builder) where T : class, IEntityEnabled
    {
        builder.Property(e => e.Enable).HasDefaultValue(true);
    }
}
