using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PGManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "Tenants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Tenants");
        }
    }
}
