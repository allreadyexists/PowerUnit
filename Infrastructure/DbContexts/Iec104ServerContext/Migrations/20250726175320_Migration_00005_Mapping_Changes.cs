using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Infrastructure.IEC104ServerDb.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00005_Mapping_Changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "source_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: -1L);

            migrationBuilder.AlterColumn<string>(
                name: "parameter_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "equipment_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "source_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                type: "bigint",
                nullable: false,
                defaultValue: -1L,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "parameter_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<long>(
                name: "equipment_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);
        }
    }
}
