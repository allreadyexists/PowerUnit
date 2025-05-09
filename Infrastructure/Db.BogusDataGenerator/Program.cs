using K2.Infrastructure.Db;

using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var context = new K2DbContextBogus(new DbContextOptions<K2DbContext>());
        {
            var equipments = await context.Equipments.ToArrayAsync();

            foreach (var equipment in equipments)
            {
                //public long Id { get; set; }
                //public EquipmentTypeEnum EquipmentTypeId { get; set; }
                //public EquipmentType EquipmentType { get; set; }
                //public string SerialNumber { get; set; } = string.Empty;
                //public string Name { get; set; }
                //public string Description { get; set; }
                Console.WriteLine($"{equipment.Id} {equipment.EquipmentTypeId} {equipment.EquipmentType} {equipment.SerialNumber} {equipment.Name} {equipment.Description}");
            }
        }
    }
}