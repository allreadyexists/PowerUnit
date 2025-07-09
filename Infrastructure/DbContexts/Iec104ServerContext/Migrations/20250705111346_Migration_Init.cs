using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class Migration_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pu_iec104_server");

            migrationBuilder.CreateTable(
                name: "iec104servers",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: ""),
                    port = table.Column<int>(type: "integer", nullable: false, defaultValue: 2404),
                    common_asdu_address = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    enable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iec104servers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "iec104types",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iec104types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "iec104server_application_layer_options",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    check_common_asdu_address = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sporadic_send_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iec104server_application_layer_options", x => x.id);
                    table.ForeignKey(
                        name: "fk_iec104server_application_layer_options_iec104servers_id",
                        column: x => x.id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "iec104servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iec104server_channel_layer_options",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    timeout0sec = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)30),
                    timeout1sec = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)15),
                    timeout2sec = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)10),
                    timeout3sec = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)20),
                    window_k_size = table.Column<int>(type: "integer", nullable: false, defaultValue: 12),
                    window_w_size = table.Column<int>(type: "integer", nullable: false, defaultValue: 8),
                    use_fragment_send = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iec104server_channel_layer_options", x => x.id);
                    table.ForeignKey(
                        name: "fk_iec104server_channel_layer_options_iec104servers_id",
                        column: x => x.id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "iec104servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iec104mappings",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    server_id = table.Column<int>(type: "integer", nullable: false),
                    equipment_id = table.Column<long>(type: "bigint", nullable: false),
                    parameter_id = table.Column<long>(type: "bigint", nullable: false),
                    address = table.Column<int>(type: "integer", nullable: false),
                    iec104type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iec104mappings", x => x.id);
                    table.ForeignKey(
                        name: "fk_iec104mappings_iec104servers_server_id",
                        column: x => x.server_id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "iec104servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_iec104mappings_iec104types_iec104type_id",
                        column: x => x.iec104type_id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "iec104types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iec104groups",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iec104mapping_id = table.Column<long>(type: "bigint", nullable: false),
                    group = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iec104groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_iec104groups_iec104mappings_iec104mapping_id",
                        column: x => x.iec104mapping_id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "iec104mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_iec104groups_group",
                schema: "pu_iec104_server",
                table: "iec104groups",
                column: "group");

            migrationBuilder.CreateIndex(
                name: "ix_iec104groups_iec104mapping_id",
                schema: "pu_iec104_server",
                table: "iec104groups",
                column: "iec104mapping_id");

            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_iec104type_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                column: "iec104type_id");

            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_server_id",
                schema: "pu_iec104_server",
                table: "iec104mappings",
                column: "server_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "iec104groups",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "iec104server_application_layer_options",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "iec104server_channel_layer_options",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "iec104mappings",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "iec104servers",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "iec104types",
                schema: "pu_iec104_server");
        }
    }
}
