using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlaystationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Device_DeviceId",
                table: "Sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Device",
                table: "Device");

            migrationBuilder.RenameTable(
                name: "Device",
                newName: "Devices");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Shifts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "SessionOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "DebtPayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Devices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Devices",
                table: "Devices",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SubscriptionType = table.Column<int>(type: "int", nullable: false),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Devices_DeviceId",
                table: "Sessions",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Devices_DeviceId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Devices",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SessionOrders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DebtPayments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Devices");

            migrationBuilder.RenameTable(
                name: "Devices",
                newName: "Device");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Device",
                table: "Device",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Device_DeviceId",
                table: "Sessions",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
