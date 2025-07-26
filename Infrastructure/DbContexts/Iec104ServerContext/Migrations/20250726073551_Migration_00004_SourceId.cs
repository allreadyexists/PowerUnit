using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Infrastructure.IEC104ServerDb.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00004_SourceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_iec104mappings_server_id_equipment_id_parameter_id_address_",
                schema: "pu_iec104_server",
                table: "iec104mappings");

            migrationBuilder.AddColumn<long>(
                name: "source_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                type: "bigint",
                nullable: false,
                defaultValue: -1L);

            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_server_id_source_id_equipment_id_parameter_i",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                columns: ["server_id", "source_id", "equipment_id", "parameter_id", "address", "iec104type_id"],
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_iec104mappings_server_id_source_id_equipment_id_parameter_i",
                schema: "pu_iec104_server",
                table: "iec104mappings");

            migrationBuilder.DropColumn(
                name: "source_id",
                schema: "pu_iec104_server",
                table: "iec104mappings");

            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_server_id_equipment_id_parameter_id_address_",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                columns: ["server_id", "equipment_id", "parameter_id", "address", "iec104type_id"],
                unique: true);
        }
    }
}
