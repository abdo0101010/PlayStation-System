using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlaystationSystem.Migrations
{
    /// <inheritdoc />
    public partial class shifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "Shifts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Shifts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBuffetIncome",
                table: "Shifts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDebtCollected",
                table: "Shifts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalExpenses",
                table: "Shifts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGamingIncome",
                table: "Shifts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "TotalBuffetIncome",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "TotalDebtCollected",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "TotalExpenses",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "TotalGamingIncome",
                table: "Shifts");
        }
    }
}
