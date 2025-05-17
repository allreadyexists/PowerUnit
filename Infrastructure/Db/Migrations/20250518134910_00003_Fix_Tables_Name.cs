using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class _00003_Fix_Tables_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_iec104server_application_layer_option_item_iec104servers_id",
                schema: "power_unit",
                table: "iec104server_application_layer_option_item");

            migrationBuilder.DropForeignKey(
                name: "fk_iec104server_channel_layer_option_iec104servers_id",
                schema: "power_unit",
                table: "iec104server_channel_layer_option");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104server_channel_layer_option",
                schema: "power_unit",
                table: "iec104server_channel_layer_option");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104server_application_layer_option_item",
                schema: "power_unit",
                table: "iec104server_application_layer_option_item");

            migrationBuilder.RenameTable(
                name: "iec104server_channel_layer_option",
                schema: "power_unit",
                newName: "iec104server_channel_layer_options",
                newSchema: "power_unit");

            migrationBuilder.RenameTable(
                name: "iec104server_application_layer_option_item",
                schema: "power_unit",
                newName: "iec104server_application_layer_options",
                newSchema: "power_unit");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104server_channel_layer_options",
                schema: "power_unit",
                table: "iec104server_channel_layer_options",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104server_application_layer_options",
                schema: "power_unit",
                table: "iec104server_application_layer_options",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_iec104server_application_layer_options_iec104servers_id",
                schema: "power_unit",
                table: "iec104server_application_layer_options",
                column: "id",
                principalSchema: "power_unit",
                principalTable: "iec104servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_iec104server_channel_layer_options_iec104servers_id",
                schema: "power_unit",
                table: "iec104server_channel_layer_options",
                column: "id",
                principalSchema: "power_unit",
                principalTable: "iec104servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_iec104server_application_layer_options_iec104servers_id",
                schema: "power_unit",
                table: "iec104server_application_layer_options");

            migrationBuilder.DropForeignKey(
                name: "fk_iec104server_channel_layer_options_iec104servers_id",
                schema: "power_unit",
                table: "iec104server_channel_layer_options");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104server_channel_layer_options",
                schema: "power_unit",
                table: "iec104server_channel_layer_options");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104server_application_layer_options",
                schema: "power_unit",
                table: "iec104server_application_layer_options");

            migrationBuilder.RenameTable(
                name: "iec104server_channel_layer_options",
                schema: "power_unit",
                newName: "iec104server_channel_layer_option",
                newSchema: "power_unit");

            migrationBuilder.RenameTable(
                name: "iec104server_application_layer_options",
                schema: "power_unit",
                newName: "iec104server_application_layer_option_item",
                newSchema: "power_unit");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104server_channel_layer_option",
                schema: "power_unit",
                table: "iec104server_channel_layer_option",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104server_application_layer_option_item",
                schema: "power_unit",
                table: "iec104server_application_layer_option_item",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_iec104server_application_layer_option_item_iec104servers_id",
                schema: "power_unit",
                table: "iec104server_application_layer_option_item",
                column: "id",
                principalSchema: "power_unit",
                principalTable: "iec104servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_iec104server_channel_layer_option_iec104servers_id",
                schema: "power_unit",
                table: "iec104server_channel_layer_option",
                column: "id",
                principalSchema: "power_unit",
                principalTable: "iec104servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
