using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Infrastructure.IEC104ServerDb.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00002_Rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_iec104groups_iec104mappings_iec104mapping_id",
                schema: "pu_iec104_server",
                table: "iec104groups");

            migrationBuilder.DropForeignKey(
                name: "fk_iec104mappings_iec104servers_server_id",
                schema: "pu_iec104_server",
                table: "iec104mappings");

            migrationBuilder.DropForeignKey(
                name: "fk_iec104mappings_iec104types_iec104type_id",
                schema: "pu_iec104_server",
                table: "iec104mappings");

            migrationBuilder.DropForeignKey(
                name: "fk_iec104servers_iec104server_application_layer_options_applic",
                schema: "pu_iec104_server",
                table: "iec104servers");

            migrationBuilder.DropForeignKey(
                name: "fk_iec104servers_iec104server_channel_layer_options_channel_la",
                schema: "pu_iec104_server",
                table: "iec104servers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104types",
                schema: "pu_iec104_server",
                table: "iec104types");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104servers",
                schema: "pu_iec104_server",
                table: "iec104servers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104server_channel_layer_options",
                schema: "pu_iec104_server",
                table: "iec104server_channel_layer_options");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104server_application_layer_options",
                schema: "pu_iec104_server",
                table: "iec104server_application_layer_options");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104mappings",
                schema: "pu_iec104_server",
                table: "iec104mappings");

            migrationBuilder.DropPrimaryKey(
                name: "pk_iec104groups",
                schema: "pu_iec104_server",
                table: "iec104groups");

            migrationBuilder.RenameTable(
                name: "iec104types",
                schema: "pu_iec104_server",
                newName: "types",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "iec104servers",
                schema: "pu_iec104_server",
                newName: "servers",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "iec104server_channel_layer_options",
                schema: "pu_iec104_server",
                newName: "channel_layer_options",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "iec104server_application_layer_options",
                schema: "pu_iec104_server",
                newName: "application_layer_options",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "iec104mappings",
                schema: "pu_iec104_server",
                newName: "mappings",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "iec104groups",
                schema: "pu_iec104_server",
                newName: "groups",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameIndex(
                name: "ix_iec104servers_channel_layer_option_id",
                schema: "pu_iec104_server",
                table: "servers",
                newName: "ix_servers_channel_layer_option_id");

            migrationBuilder.RenameIndex(
                name: "ix_iec104servers_application_layer_option_id",
                schema: "pu_iec104_server",
                table: "servers",
                newName: "ix_servers_application_layer_option_id");

            migrationBuilder.RenameIndex(
                name: "ix_iec104mappings_server_id_source_id_equipment_id_parameter_i",
                schema: "pu_iec104_server",
                table: "mappings",
                newName: "ix_mappings_server_id_source_id_equipment_id_parameter_id_addr");

            migrationBuilder.RenameIndex(
                name: "ix_iec104mappings_iec104type_id",
                schema: "pu_iec104_server",
                table: "mappings",
                newName: "ix_mappings_iec104type_id");

            migrationBuilder.RenameIndex(
                name: "ix_iec104groups_iec104mapping_id",
                schema: "pu_iec104_server",
                table: "groups",
                newName: "ix_groups_iec104mapping_id");

            migrationBuilder.RenameIndex(
                name: "ix_iec104groups_group",
                schema: "pu_iec104_server",
                table: "groups",
                newName: "ix_groups_group");

            migrationBuilder.AddPrimaryKey(
                name: "pk_types",
                schema: "pu_iec104_server",
                table: "types",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_servers",
                schema: "pu_iec104_server",
                table: "servers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_channel_layer_options",
                schema: "pu_iec104_server",
                table: "channel_layer_options",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_application_layer_options",
                schema: "pu_iec104_server",
                table: "application_layer_options",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_mappings",
                schema: "pu_iec104_server",
                table: "mappings",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_groups",
                schema: "pu_iec104_server",
                table: "groups",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_groups_mappings_iec104mapping_id",
                schema: "pu_iec104_server",
                table: "groups",
                column: "iec104mapping_id",
                principalSchema: "pu_iec104_server",
                principalTable: "mappings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_mappings_servers_server_id",
                schema: "pu_iec104_server",
                table: "mappings",
                column: "server_id",
                principalSchema: "pu_iec104_server",
                principalTable: "servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_mappings_types_iec104type_id",
                schema: "pu_iec104_server",
                table: "mappings",
                column: "iec104type_id",
                principalSchema: "pu_iec104_server",
                principalTable: "types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_servers_application_layer_options_application_layer_option_",
                schema: "pu_iec104_server",
                table: "servers",
                column: "application_layer_option_id",
                principalSchema: "pu_iec104_server",
                principalTable: "application_layer_options",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_servers_channel_layer_options_channel_layer_option_id",
                schema: "pu_iec104_server",
                table: "servers",
                column: "channel_layer_option_id",
                principalSchema: "pu_iec104_server",
                principalTable: "channel_layer_options",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_groups_mappings_iec104mapping_id",
                schema: "pu_iec104_server",
                table: "groups");

            migrationBuilder.DropForeignKey(
                name: "fk_mappings_servers_server_id",
                schema: "pu_iec104_server",
                table: "mappings");

            migrationBuilder.DropForeignKey(
                name: "fk_mappings_types_iec104type_id",
                schema: "pu_iec104_server",
                table: "mappings");

            migrationBuilder.DropForeignKey(
                name: "fk_servers_application_layer_options_application_layer_option_",
                schema: "pu_iec104_server",
                table: "servers");

            migrationBuilder.DropForeignKey(
                name: "fk_servers_channel_layer_options_channel_layer_option_id",
                schema: "pu_iec104_server",
                table: "servers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_types",
                schema: "pu_iec104_server",
                table: "types");

            migrationBuilder.DropPrimaryKey(
                name: "pk_servers",
                schema: "pu_iec104_server",
                table: "servers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_mappings",
                schema: "pu_iec104_server",
                table: "mappings");

            migrationBuilder.DropPrimaryKey(
                name: "pk_groups",
                schema: "pu_iec104_server",
                table: "groups");

            migrationBuilder.DropPrimaryKey(
                name: "pk_channel_layer_options",
                schema: "pu_iec104_server",
                table: "channel_layer_options");

            migrationBuilder.DropPrimaryKey(
                name: "pk_application_layer_options",
                schema: "pu_iec104_server",
                table: "application_layer_options");

            migrationBuilder.RenameTable(
                name: "types",
                schema: "pu_iec104_server",
                newName: "iec104types",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "servers",
                schema: "pu_iec104_server",
                newName: "iec104servers",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "mappings",
                schema: "pu_iec104_server",
                newName: "iec104mappings",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "groups",
                schema: "pu_iec104_server",
                newName: "iec104groups",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "channel_layer_options",
                schema: "pu_iec104_server",
                newName: "iec104server_channel_layer_options",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameTable(
                name: "application_layer_options",
                schema: "pu_iec104_server",
                newName: "iec104server_application_layer_options",
                newSchema: "pu_iec104_server");

            migrationBuilder.RenameIndex(
                name: "ix_servers_channel_layer_option_id",
                schema: "pu_iec104_server",
                table: "iec104servers",
                newName: "ix_iec104servers_channel_layer_option_id");

            migrationBuilder.RenameIndex(
                name: "ix_servers_application_layer_option_id",
                schema: "pu_iec104_server",
                table: "iec104servers",
                newName: "ix_iec104servers_application_layer_option_id");

            migrationBuilder.RenameIndex(
                name: "ix_mappings_server_id_source_id_equipment_id_parameter_id_addr",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                newName: "ix_iec104mappings_server_id_source_id_equipment_id_parameter_i");

            migrationBuilder.RenameIndex(
                name: "ix_mappings_iec104type_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                newName: "ix_iec104mappings_iec104type_id");

            migrationBuilder.RenameIndex(
                name: "ix_groups_iec104mapping_id",
                schema: "pu_iec104_server",
                table: "iec104groups",
                newName: "ix_iec104groups_iec104mapping_id");

            migrationBuilder.RenameIndex(
                name: "ix_groups_group",
                schema: "pu_iec104_server",
                table: "iec104groups",
                newName: "ix_iec104groups_group");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104types",
                schema: "pu_iec104_server",
                table: "iec104types",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104servers",
                schema: "pu_iec104_server",
                table: "iec104servers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104mappings",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104groups",
                schema: "pu_iec104_server",
                table: "iec104groups",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104server_channel_layer_options",
                schema: "pu_iec104_server",
                table: "iec104server_channel_layer_options",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_iec104server_application_layer_options",
                schema: "pu_iec104_server",
                table: "iec104server_application_layer_options",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_iec104groups_iec104mappings_iec104mapping_id",
                schema: "pu_iec104_server",
                table: "iec104groups",
                column: "iec104mapping_id",
                principalSchema: "pu_iec104_server",
                principalTable: "iec104mappings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_iec104mappings_iec104servers_server_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                column: "server_id",
                principalSchema: "pu_iec104_server",
                principalTable: "iec104servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_iec104mappings_iec104types_iec104type_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                column: "iec104type_id",
                principalSchema: "pu_iec104_server",
                principalTable: "iec104types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_iec104servers_iec104server_application_layer_options_applic",
                schema: "pu_iec104_server",
                table: "iec104servers",
                column: "application_layer_option_id",
                principalSchema: "pu_iec104_server",
                principalTable: "iec104server_application_layer_options",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_iec104servers_iec104server_channel_layer_options_channel_la",
                schema: "pu_iec104_server",
                table: "iec104servers",
                column: "channel_layer_option_id",
                principalSchema: "pu_iec104_server",
                principalTable: "iec104server_channel_layer_options",
                principalColumn: "id");
        }
    }
}
