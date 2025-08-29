using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00001_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pu_iec104_server");

            migrationBuilder.CreateTable(
                name: "application_layer_options",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    check_common_asdu_address = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sporadic_send_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_layer_options", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "channel_layer_options",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timeout0sec = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)30),
                    timeout1sec = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)15),
                    timeout2sec = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)10),
                    timeout3sec = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)20),
                    window_k_size = table.Column<int>(type: "integer", nullable: false, defaultValue: 12),
                    window_w_size = table.Column<int>(type: "integer", nullable: false, defaultValue: 8),
                    use_fragment_send = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    max_queue_size = table.Column<int>(type: "integer", nullable: false, defaultValue: 100)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_layer_options", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "types",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servers",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: ""),
                    port = table.Column<int>(type: "integer", nullable: false, defaultValue: 2404),
                    common_asdu_address = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    enable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    application_layer_option_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    channel_layer_option_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_servers", x => x.id);
                    table.ForeignKey(
                        name: "fk_servers_application_layer_options_application_layer_option_",
                        column: x => x.application_layer_option_id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "application_layer_options",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_servers_channel_layer_options_channel_layer_option_id",
                        column: x => x.channel_layer_option_id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "channel_layer_options",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mappings",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    server_id = table.Column<int>(type: "integer", nullable: false),
                    source_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValue: ""),
                    equipment_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    parameter_id = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    address = table.Column<int>(type: "integer", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mappings", x => x.id);
                    table.ForeignKey(
                        name: "fk_mappings_servers_server_id",
                        column: x => x.server_id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mappings_types_type_id",
                        column: x => x.type_id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                schema: "pu_iec104_server",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mapping_id = table.Column<long>(type: "bigint", nullable: false),
                    group = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_groups_mappings_mapping_id",
                        column: x => x.mapping_id,
                        principalSchema: "pu_iec104_server",
                        principalTable: "mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_groups_mapping_id",
                schema: "pu_iec104_server",
                table: "groups",
                column: "mapping_id");

            migrationBuilder.CreateIndex(
                name: "ix_mappings_server_id_source_id_equipment_id_parameter_id_addr",
                schema: "pu_iec104_server",
                table: "mappings",
                columns: ["server_id", "source_id", "equipment_id", "parameter_id", "address", "type_id"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mappings_type_id",
                schema: "pu_iec104_server",
                table: "mappings",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_servers_application_layer_option_id",
                schema: "pu_iec104_server",
                table: "servers",
                column: "application_layer_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_servers_channel_layer_option_id",
                schema: "pu_iec104_server",
                table: "servers",
                column: "channel_layer_option_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "groups",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "mappings",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "servers",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "types",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "application_layer_options",
                schema: "pu_iec104_server");

            migrationBuilder.DropTable(
                name: "channel_layer_options",
                schema: "pu_iec104_server");
        }
    }
}
