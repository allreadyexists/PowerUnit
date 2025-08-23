using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Infrastructure.IEC104ServerDb.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00005_Append_Content_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = TargetModel.GetDefaultSchema();
            migrationBuilder.Sql($@"
                INSERT INTO {schema}.servers(name, port, common_asdu_address, enable, application_layer_option_id, channel_layer_option_id)
                VALUES ('Default server', 2404, 1, 'true', 1, 1)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var schema = TargetModel.GetDefaultSchema();
            migrationBuilder.Sql($@"DELETE FROM {schema}.servers");
        }
    }
}
