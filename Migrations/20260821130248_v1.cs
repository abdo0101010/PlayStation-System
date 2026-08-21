using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlaystationSystem.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "DebtPayments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionEndDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionStartDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionType",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DebtPayments_CustomerId",
                table: "DebtPayments",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DebtPayments_Customer_CustomerId",
                table: "DebtPayments",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DebtPayments_Customer_CustomerId",
                table: "DebtPayments");

            migrationBuilder.DropIndex(
                name: "IX_DebtPayments_CustomerId",
                table: "DebtPayments");

            migrationBuilder.DropColumn(
                name: "SubscriptionEndDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscriptionStartDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscriptionType",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "DebtPayments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
