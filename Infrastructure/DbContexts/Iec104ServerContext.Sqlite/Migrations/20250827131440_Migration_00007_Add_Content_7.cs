using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00007_Add_Content_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DELETE FROM servers");
            migrationBuilder.Sql($@"DELETE FROM channel_layer_options");
            migrationBuilder.Sql($@"DELETE FROM application_layer_options");

            migrationBuilder.Sql($@"
                INSERT INTO application_layer_options(id, check_common_asdu_address, sporadic_send_enabled) VALUES
                (1, 1, 1)
                ");
            migrationBuilder.Sql($@"
                INSERT INTO channel_layer_options(id, timeout0sec, timeout1sec, timeout2sec, timeout3sec, window_k_size, window_w_size, use_fragment_send, max_queue_size) VALUES
                (1, 30, 15, 10, 20, 12, 8, 0, 100)
                ");
            migrationBuilder.Sql($@"
                INSERT INTO servers(name, port, common_asdu_address, enable, application_layer_option_id, channel_layer_option_id)
                VALUES ('Default server', 2404, 1, 1, 1, 1)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
