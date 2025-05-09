using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "power_unit");

            migrationBuilder.CreateTable(
                name: "discret_types",
                schema: "power_unit",
                columns: table => new
                {
                    id = table.Column<byte>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discret_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipment_types",
                schema: "power_unit",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "iec104servers",
                schema: "power_unit",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: ""),
                    port = table.Column<int>(type: "integer", nullable: false, defaultValue: 2404),
                    common_asdu_address = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    check_common_asdu_address = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sporadic_send_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    file_section_size = table.Column<int>(type: "integer", maxLength: 4096, nullable: false, defaultValue: 1024),
                    file_segment_size = table.Column<byte>(type: "smallint", maxLength: 200, nullable: false, defaultValue: (byte)200),
                    enable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iec104servers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "iec104types",
                schema: "power_unit",
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
                name: "measurement_types",
                schema: "power_unit",
                columns: table => new
                {
                    id = table.Column<byte>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_measurement_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipments",
                schema: "power_unit",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    equipment_type_id = table.Column<int>(type: "integer", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: ""),
                    description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipments", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipments_equipment_types_equipment_type_id",
                        column: x => x.equipment_type_id,
                        principalSchema: "power_unit",
                        principalTable: "equipment_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iec104server_channel_layer_option",
                schema: "power_unit",
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
                    table.PrimaryKey("pk_iec104server_channel_layer_option", x => x.id);
                    table.ForeignKey(
                        name: "fk_iec104server_channel_layer_option_iec104servers_id",
                        column: x => x.id,
                        principalSchema: "power_unit",
                        principalTable: "iec104servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parameter_types",
                schema: "power_unit",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: ""),
                    measurement_type_id = table.Column<byte>(type: "smallint", nullable: false),
                    discret_type_id = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parameter_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_parameter_types_discret_types_discret_type_id",
                        column: x => x.discret_type_id,
                        principalSchema: "power_unit",
                        principalTable: "discret_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_parameter_types_measurement_types_measurement_type_id",
                        column: x => x.measurement_type_id,
                        principalSchema: "power_unit",
                        principalTable: "measurement_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iec104mappings",
                schema: "power_unit",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    server_id = table.Column<int>(type: "integer", nullable: false),
                    equipment_id = table.Column<long>(type: "bigint", nullable: false),
                    parameter_type_id = table.Column<int>(type: "integer", nullable: false),
                    address = table.Column<int>(type: "integer", nullable: false),
                    iec104type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iec104mappings", x => x.id);
                    table.ForeignKey(
                        name: "fk_iec104mappings_equipments_equipment_id",
                        column: x => x.equipment_id,
                        principalSchema: "power_unit",
                        principalTable: "equipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_iec104mappings_iec104servers_server_id",
                        column: x => x.server_id,
                        principalSchema: "power_unit",
                        principalTable: "iec104servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_iec104mappings_iec104types_iec104type_id",
                        column: x => x.iec104type_id,
                        principalSchema: "power_unit",
                        principalTable: "iec104types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_iec104mappings_parameter_types_parameter_type_id",
                        column: x => x.parameter_type_id,
                        principalSchema: "power_unit",
                        principalTable: "parameter_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "measurements",
                schema: "power_unit",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    equipment_id = table.Column<long>(type: "bigint", nullable: false),
                    parameter_type_id = table.Column<int>(type: "integer", nullable: false),
                    value_dt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    registration_dt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_measurements", x => x.id);
                    table.ForeignKey(
                        name: "fk_measurements_equipments_equipment_id",
                        column: x => x.equipment_id,
                        principalSchema: "power_unit",
                        principalTable: "equipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_measurements_parameter_types_parameter_type_id",
                        column: x => x.parameter_type_id,
                        principalSchema: "power_unit",
                        principalTable: "parameter_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iec104groups",
                schema: "power_unit",
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
                        principalSchema: "power_unit",
                        principalTable: "iec104mappings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable CA1861 // Avoid constant arrays as arguments
            migrationBuilder.CreateIndex(
                name: "ix_equipments_equipment_type_id_serial_number",
                schema: "power_unit",
                table: "equipments",
                columns: new[] { "equipment_type_id", "serial_number" },
                unique: true);
#pragma warning restore CA1861 // Avoid constant arrays as arguments
#pragma warning restore IDE0300 // Simplify collection initialization

            migrationBuilder.CreateIndex(
                name: "ix_equipments_serial_number",
                schema: "power_unit",
                table: "equipments",
                column: "serial_number");

            migrationBuilder.CreateIndex(
                name: "ix_iec104groups_group",
                schema: "power_unit",
                table: "iec104groups",
                column: "group");

            migrationBuilder.CreateIndex(
                name: "ix_iec104groups_iec104mapping_id",
                schema: "power_unit",
                table: "iec104groups",
                column: "iec104mapping_id");

            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_equipment_id",
                schema: "power_unit",
                table: "iec104mappings",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_iec104type_id",
                schema: "power_unit",
                table: "iec104mappings",
                column: "iec104type_id");

            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_parameter_type_id",
                schema: "power_unit",
                table: "iec104mappings",
                column: "parameter_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_iec104mappings_server_id",
                schema: "power_unit",
                table: "iec104mappings",
                column: "server_id");

            migrationBuilder.CreateIndex(
                name: "ix_measurements_equipment_id",
                schema: "power_unit",
                table: "measurements",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_measurements_parameter_type_id",
                schema: "power_unit",
                table: "measurements",
                column: "parameter_type_id");

#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable CA1861 // Avoid constant arrays as arguments
            migrationBuilder.CreateIndex(
                name: "ix_measurements_value_dt_equipment_id_parameter_type_id",
                schema: "power_unit",
                table: "measurements",
                columns: new[] { "value_dt", "equipment_id", "parameter_type_id" },
                unique: true);
#pragma warning restore CA1861 // Avoid constant arrays as arguments
#pragma warning restore IDE0300 // Simplify collection initialization

            migrationBuilder.CreateIndex(
                name: "ix_parameter_types_discret_type_id",
                schema: "power_unit",
                table: "parameter_types",
                column: "discret_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_parameter_types_measurement_type_id",
                schema: "power_unit",
                table: "parameter_types",
                column: "measurement_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "iec104groups",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "iec104server_channel_layer_option",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "measurements",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "iec104mappings",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "equipments",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "iec104servers",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "iec104types",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "parameter_types",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "equipment_types",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "discret_types",
                schema: "power_unit");

            migrationBuilder.DropTable(
                name: "measurement_types",
                schema: "power_unit");
        }
    }
}
