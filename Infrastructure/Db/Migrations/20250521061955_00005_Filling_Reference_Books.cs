using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class _00005_Filling_Reference_Books : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = base.TargetModel.GetDefaultSchema();
            migrationBuilder.Sql(
                @$"INSERT INTO power_unit.equipment_types(""id"", description) VALUES
                    (0, 'Spodes1'),
                    (1, 'Spodes3')
                "
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
