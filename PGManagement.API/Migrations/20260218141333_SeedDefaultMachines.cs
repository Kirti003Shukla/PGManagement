using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PGManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultMachines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Machines",
                columns: new[] { "MachineId", "CurrentUserId", "EndTime", "IsAvailable", "MachineName" },
                values: new object[,]
                {
                    { 1, null, null, true, "Machine 1" },
                    { 2, null, null, true, "Machine 2" },
                    { 3, null, null, true, "Machine 3" },
                    { 4, null, null, true, "Machine 4" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Machines",
                keyColumn: "MachineId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Machines",
                keyColumn: "MachineId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Machines",
                keyColumn: "MachineId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Machines",
                keyColumn: "MachineId",
                keyValue: 4);
        }
    }
}
