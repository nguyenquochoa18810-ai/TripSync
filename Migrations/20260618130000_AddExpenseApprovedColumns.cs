using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTQL_DU_LICH.Migrations
{
    // Migration vá: thêm 2 cột IsApproved + ApprovedAt vào bảng Expenses.
    // Model Expense đã khai báo 2 cột này nhưng trước đó không migration nào tạo chúng,
    // nên DB luôn báo lỗi "Invalid column name 'IsApproved'".
    public partial class AddExpenseApprovedColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Expenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Expenses",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsApproved", table: "Expenses");
            migrationBuilder.DropColumn(name: "ApprovedAt", table: "Expenses");
        }
    }
}