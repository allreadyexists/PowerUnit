using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class _00002_Separate_IEC104_AppProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "check_common_asdu_address",
                schema: "power_unit",
                table: "iec104servers");

            migrationBuilder.DropColumn(
                name: "file_section_size",
                schema: "power_unit",
                table: "iec104servers");

            migrationBuilder.DropColumn(
                name: "file_segment_size",
                schema: "power_unit",
                table: "iec104servers");

            migrationBuilder.DropColumn(
                name: "sporadic_send_enabled",
                schema: "power_unit",
                table: "iec104servers");

            migrationBuilder.CreateTable(
                name: "iec104server_application_layer_option_item",
                schema: "power_unit",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    check_common_asdu_address = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sporadic_send_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    file_segment_size = table.Column<byte>(type: "smallint", maxLength: 200, nullable: false, defaultValue: (byte)200)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iec104server_application_layer_option_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_iec104server_application_layer_option_item_iec104servers_id",
                        column: x => x.id,
                        principalSchema: "power_unit",
                        principalTable: "iec104servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "iec104server_application_layer_option_item",
                schema: "power_unit");

            migrationBuilder.AddColumn<bool>(
                name: "check_common_asdu_address",
                schema: "power_unit",
                table: "iec104servers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "file_section_size",
                schema: "power_unit",
                table: "iec104servers",
                type: "integer",
                maxLength: 4096,
                nullable: false,
                defaultValue: 1024);

            migrationBuilder.AddColumn<byte>(
                name: "file_segment_size",
                schema: "power_unit",
                table: "iec104servers",
                type: "smallint",
                maxLength: 200,
                nullable: false,
                defaultValue: (byte)200);

            migrationBuilder.AddColumn<bool>(
                name: "sporadic_send_enabled",
                schema: "power_unit",
                table: "iec104servers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
