using K2.Infrastructure.Db;

using Microsoft.EntityFrameworkCore;

internal class K2DbContextBogus : K2DbContext
{
    public K2DbContextBogus(DbContextOptions<K2DbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        FakeEquipments.Init(10);

        modelBuilder.Entity<Equipment>().HasData(FakeEquipments.Equipments);
    }

    private static class FakeEquipments
    {
        public static List<Equipment> Equipments { get; } = [];

        public static void Init(int count)
        {

        }
    }
}
