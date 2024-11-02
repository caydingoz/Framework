using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.AuthServer.Migrations
{
    /// <inheritdoc />
    public partial class Absence_property_type_set_to_double : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "TotalAbsenceEntitlement",
                table: "Users",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<double>(
                name: "Duration",
                table: "Absence",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "TotalAbsenceEntitlement",
                table: "Users",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Duration",
                table: "Absence",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
