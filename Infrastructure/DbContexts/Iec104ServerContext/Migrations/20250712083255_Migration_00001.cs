using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Infrastructure.IEC104ServerDb.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_iec104mappings_server_id",
                schema: "pu_iec104_server",
                table: "iec104mappings");

#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable CA1861 // Avoid constant arrays as arguments
            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_server_id_equipment_id_parameter_id_address_",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                columns: new[] { "server_id", "equipment_id", "parameter_id", "address", "iec104type_id" },
                unique: true);
#pragma warning restore CA1861 // Avoid constant arrays as arguments
#pragma warning restore IDE0300 // Simplify collection initialization
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_iec104mappings_server_id_equipment_id_parameter_id_address_",
                schema: "pu_iec104_server",
                table: "iec104mappings");

            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_server_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                column: "server_id");
        }
    }
}
