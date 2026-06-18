using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTQL_DU_LICH.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServiceRequestIdFromNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceRequestId",
                table: "Notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceRequestId",
                table: "Notifications",
                type: "int",
                nullable: true);
        }
    }
}
