using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Migrations
{
    /// <inheritdoc />
    public partial class _00004_Filling_Reference_Books : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = base.TargetModel.GetDefaultSchema();
            migrationBuilder.Sql(
                @$"INSERT INTO {schema}.measurement_types(""id"", description) VALUES
                    (0, 'Instant'),
                    (1, 'Current'),
                    (2, 'Integral'),
                    (3, 'Differential')
                "
            );

            migrationBuilder.Sql(
                @$"INSERT INTO {schema}.discret_types(""id"", description) VALUES
                    (0, 'None'),
                    (1, 'DayFix'),
                    (2, 'MonthFix'),
                    (3, 'YearFix'),
                    (4, 'Min1'),
                    (5, 'Min2'),
                    (6, 'Min3'),
                    (7, 'Min4'),
                    (8, 'Min5'),
                    (9, 'Min6'),
                    (10, 'Min10'),
                    (11, 'Min12'),
                    (12, 'Min15'),
                    (13, 'Min20'),
                    (14, 'Min30'),
                    (15, 'Hour1')
                "
            );

            migrationBuilder.Sql(
                @$"INSERT INTO {schema}.parameter_types(""id"", description, measurement_type_id, discret_type_id) VALUES
                    (0, 'Freq Instant', 0, 0),

                    (1, 'Voltage Instant', 0, 0),
                    (2, 'VoltagePhaseA Instant', 0, 0),
                    (3, 'VoltagePhaseB Instant', 0, 0),
                    (4, 'VoltagePhaseC Instant', 0, 0),

                    (5, 'Current Instant', 0, 0),
                    (6, 'CurrentPhaseA Instant', 0, 0),
                    (7, 'CurrentPhaseB Instant', 0, 0),
                    (8, 'CurrentPhaseC Instant', 0, 0)
                "
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
