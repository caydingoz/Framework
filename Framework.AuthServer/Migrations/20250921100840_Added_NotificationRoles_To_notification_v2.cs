using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.AuthServer.Migrations
{
    /// <inheritdoc />
    public partial class Added_NotificationRoles_To_notification_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRole_RoleId",
                table: "NotificationRole",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationRole_Roles_RoleId",
                table: "NotificationRole",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationRole_Roles_RoleId",
                table: "NotificationRole");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRole_RoleId",
                table: "NotificationRole");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Notifications");
        }
    }
}
