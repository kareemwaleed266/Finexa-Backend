using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finexa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToBillSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BillSeries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BillSeries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_BillSeries_AppUserId_IsDeleted",
                table: "BillSeries",
                columns: new[] { "AppUserId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BillSeries_AppUserId_IsDeleted",
                table: "BillSeries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BillSeries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BillSeries");
        }
    }
}
