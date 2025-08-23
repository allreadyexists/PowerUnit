using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerUnit.Infrastructure.IEC104ServerDb.Migrations
{
    /// <inheritdoc />
    public partial class Migration_00004_Append_Content : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = TargetModel.GetDefaultSchema();
            migrationBuilder.Sql($@"
                INSERT INTO {schema}.types(id, name) VALUES
                (1, 'M_SP_NA_1'),
                (2, 'M_SP_TA_1'),
                (3, 'M_DP_NA_1'),
                (4, 'M_DP_TA_1'),
                (5, 'M_ST_NA_1'),
                (6, 'M_ST_TA_1'),
                (7, 'M_BO_NA_1'),
                (8, 'М_ВО_ТА_1'),
                (9, 'M_ME_NA_1'),
                (10, 'M_ME_TA_1'),
                (11, 'M_ME_NB_1'),
                (12, 'M_ME_TB_1'),
                (13, 'M_ME_NC_1'),
                (14, 'M_ME_TC_1'),
                (15, 'M_IT_NA_1'),
                (16, 'M_IT_TA_1'),
                (17, 'М_ЕР_ТА_1'),
                (18, 'М_ЕР_ТВ_1'),
                (19, 'М_ЕР_ТС_1'),
                (20, 'M_PS_NA_1'),
                (21, 'M_ME_ND_1'),
                (30, 'M_SP_TB_1'),
                (31, 'M_DP_TB_1'),
                (32, 'M_ST_TB_1'),
                (33, 'М_ВО_ТВ_1'),
                (34, 'M_ME_TD_1'),
                (35, 'M_ME_TE_1'),
                (36, 'M_ME_TF_1'),
                (37, 'M_IT_TB_1'),
                (38, 'M_EP_TD_1'),
                (39, 'М_ЕР_ТЕ_1'),
                (40, 'M_EP_TF_1'),
                (45, 'C_SC_NA_1'),
                (46, 'C_DC_NA_1'),
                (47, 'C_RC_NA_1'),
                (48, 'C_SE_NA_1'),
                (49, 'C_SE_NB_1'),
                (50, 'C_SE_NC_1'),
                (51, 'C_BO_NA_1'),
                (58, 'C_SC_TA_1'),
                (59, 'C_DC_TA_1'),
                (60, 'C_RC_TA_1'),
                (61, 'C_SE_TA_1'),
                (62, 'C_SE_TB_1'),
                (63, 'C_SE_TC_1'),
                (64, 'С_ВО_ТА_1'),
                (70, 'M_EI_NA_1'),
                (100, 'C_IC_NA_1'),
                (101, 'C_CI_NA_1'),
                (102, 'C_RD_NA_1'),
                (103, 'C_CS_NA_1'),
                (104, 'C_TS_NA_1'),
                (105, 'C_RP_NA_1'),
                (106, 'C_CD_NA_1'),
                (107, 'C_TS_TA_1'),
                (110, 'P_ME_NA_1'),
                (111, 'P_ME_NB_1'),
                (112, 'P_ME_NC_1'),
                (113, 'P_AC_NA_1'),
                (120, 'F_FR_NA_1'),
                (121, 'F_SR_NA_1'),
                (122, 'F_SC_NA_1'),
                (123, 'F_LS_NA_1'),
                (124, 'F_AF_NA_1'),
                (125, 'F_SG_NA_1'),
                (126, 'F_DR_TA_1')");
            migrationBuilder.Sql($@"
                INSERT INTO {schema}.application_layer_options(id, check_common_asdu_address, sporadic_send_enabled) VALUES
                (1, 'true', 'true')
                ");
            migrationBuilder.Sql($@"
                INSERT INTO {schema}.channel_layer_options(id, timeout0sec, timeout1sec, timeout2sec, timeout3sec, window_k_size, window_w_size, use_fragment_send, max_queue_size) VALUES
                (1, 30, 15, 10, 20, 12, 8, 'false', 100)
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var schema = TargetModel.GetDefaultSchema();
            migrationBuilder.Sql($@"DELETE FROM {schema}.types");
            migrationBuilder.Sql($@"DELETE FROM {schema}.application_layer_options");
            migrationBuilder.Sql($@"DELETE FROM {schema}.channel_layer_options");
        }
    }
}
