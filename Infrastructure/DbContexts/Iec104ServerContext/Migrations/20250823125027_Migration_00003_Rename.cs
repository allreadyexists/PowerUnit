using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Infrastructure.IEC104ServerDb.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00003_Rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_groups_mappings_iec104mapping_id",
                schema: "pu_iec104_server",
                table: "groups");

            migrationBuilder.DropForeignKey(
                name: "fk_mappings_types_iec104type_id",
                schema: "pu_iec104_server",
                table: "mappings");

            migrationBuilder.RenameColumn(
                name: "iec104type_id",
                schema: "pu_iec104_server",
                table: "mappings",
                newName: "type_id");

            migrationBuilder.RenameIndex(
                name: "ix_mappings_iec104type_id",
                schema: "pu_iec104_server",
                table: "mappings",
                newName: "ix_mappings_type_id");

            migrationBuilder.RenameColumn(
                name: "iec104mapping_id",
                schema: "pu_iec104_server",
                table: "groups",
                newName: "mapping_id");

            migrationBuilder.RenameIndex(
                name: "ix_groups_iec104mapping_id",
                schema: "pu_iec104_server",
                table: "groups",
                newName: "ix_groups_mapping_id");

            migrationBuilder.AddForeignKey(
                name: "fk_groups_mappings_mapping_id",
                schema: "pu_iec104_server",
                table: "groups",
                column: "mapping_id",
                principalSchema: "pu_iec104_server",
                principalTable: "mappings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_mappings_types_type_id",
                schema: "pu_iec104_server",
                table: "mappings",
                column: "type_id",
                principalSchema: "pu_iec104_server",
                principalTable: "types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_groups_mappings_mapping_id",
                schema: "pu_iec104_server",
                table: "groups");

            migrationBuilder.DropForeignKey(
                name: "fk_mappings_types_type_id",
                schema: "pu_iec104_server",
                table: "mappings");

            migrationBuilder.RenameColumn(
                name: "type_id",
                schema: "pu_iec104_server",
                table: "mappings",
                newName: "iec104type_id");

            migrationBuilder.RenameIndex(
                name: "ix_mappings_type_id",
                schema: "pu_iec104_server",
                table: "mappings",
                newName: "ix_mappings_iec104type_id");

            migrationBuilder.RenameColumn(
                name: "mapping_id",
                schema: "pu_iec104_server",
                table: "groups",
                newName: "iec104mapping_id");

            migrationBuilder.RenameIndex(
                name: "ix_groups_mapping_id",
                schema: "pu_iec104_server",
                table: "groups",
                newName: "ix_groups_iec104mapping_id");

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
                name: "fk_mappings_types_iec104type_id",
                schema: "pu_iec104_server",
                table: "mappings",
                column: "iec104type_id",
                principalSchema: "pu_iec104_server",
                principalTable: "types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
