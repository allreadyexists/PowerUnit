using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00003_Add_Content_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "enable",
                table: "servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "use_fragment_send",
                table: "channel_layer_options",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "sporadic_send_enabled",
                table: "application_layer_options",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "check_common_asdu_address",
                table: "application_layer_options",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "enable",
                table: "servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "use_fragment_send",
                table: "channel_layer_options",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "sporadic_send_enabled",
                table: "application_layer_options",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "check_common_asdu_address",
                table: "application_layer_options",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 1);
        }
    }
}
