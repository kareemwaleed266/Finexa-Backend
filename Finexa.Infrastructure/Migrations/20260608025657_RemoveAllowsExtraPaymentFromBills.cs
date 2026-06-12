using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finexa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAllowsExtraPaymentFromBills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowsExtraPayment",
                table: "BillSeries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowsExtraPayment",
                table: "BillSeries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
