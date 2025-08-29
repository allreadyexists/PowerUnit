using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00001_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "application_layer_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    check_common_asdu_address = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    sporadic_send_enabled = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_layer_options", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "channel_layer_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    timeout0sec = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)30),
                    timeout1sec = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)15),
                    timeout2sec = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)10),
                    timeout3sec = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)20),
                    window_k_size = table.Column<ushort>(type: "INTEGER", nullable: false, defaultValue: (ushort)12),
                    window_w_size = table.Column<ushort>(type: "INTEGER", nullable: false, defaultValue: (ushort)8),
                    use_fragment_send = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    max_queue_size = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_layer_options", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "types",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servers",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, defaultValue: ""),
                    port = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 2404),
                    common_asdu_address = table.Column<ushort>(type: "INTEGER", nullable: false, defaultValue: (ushort)1),
                    enable = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    application_layer_option_id = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    channel_layer_option_id = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_servers", x => x.id);
                    table.ForeignKey(
                        name: "fk_servers_application_layer_options_application_layer_option_id",
                        column: x => x.application_layer_option_id,
                        principalTable: "application_layer_options",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_servers_channel_layer_options_channel_layer_option_id",
                        column: x => x.channel_layer_option_id,
                        principalTable: "channel_layer_options",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mappings",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    server_id = table.Column<int>(type: "INTEGER", nullable: false),
                    source_id = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false, defaultValue: ""),
                    equipment_id = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    parameter_id = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    address = table.Column<ushort>(type: "INTEGER", nullable: false),
                    type_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mappings", x => x.id);
                    table.ForeignKey(
                        name: "fk_mappings_servers_server_id",
                        column: x => x.server_id,
                        principalTable: "servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mappings_types_type_id",
                        column: x => x.type_id,
                        principalTable: "types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    mapping_id = table.Column<long>(type: "INTEGER", nullable: false),
                    group = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_groups_mappings_mapping_id",
                        column: x => x.mapping_id,
                        principalTable: "mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_groups_mapping_id",
                table: "groups",
                column: "mapping_id");

            migrationBuilder.CreateIndex(
                name: "ix_mappings_server_id_source_id_equipment_id_parameter_id_address_type_id",
                table: "mappings",
                columns: ["server_id", "source_id", "equipment_id", "parameter_id", "address", "type_id"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mappings_type_id",
                table: "mappings",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_servers_application_layer_option_id",
                table: "servers",
                column: "application_layer_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_servers_channel_layer_option_id",
                table: "servers",
                column: "channel_layer_option_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "mappings");

            migrationBuilder.DropTable(
                name: "servers");

            migrationBuilder.DropTable(
                name: "types");

            migrationBuilder.DropTable(
                name: "application_layer_options");

            migrationBuilder.DropTable(
                name: "channel_layer_options");
        }
    }
}
