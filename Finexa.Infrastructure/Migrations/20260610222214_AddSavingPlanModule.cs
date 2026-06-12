using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finexa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingPlanModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavingPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisPeriodMonths = table.Column<int>(type: "int", nullable: false),
                    PlanType = table.Column<int>(type: "int", nullable: false),
                    TargetMonthlySaving = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AverageIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentAverageSaving = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForecastedIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForecastedExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForecastedSaving = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecommendedMonthlySaving = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExtraSavingOpportunity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PlanStatusLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SummaryMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingPlans_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingPlanItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SavingPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CategoryType = table.Column<int>(type: "int", nullable: false),
                    CurrentAverage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecommendedBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReductionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ExpectedSaving = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingPlanItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavingPlanItems_SavingPlans_SavingPlanId",
                        column: x => x.SavingPlanId,
                        principalTable: "SavingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingPlanMonthlyProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SavingPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    RecommendedMonthlySaving = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualSaving = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProgressPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingPlanMonthlyProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingPlanMonthlyProgress_SavingPlans_SavingPlanId",
                        column: x => x.SavingPlanId,
                        principalTable: "SavingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavingPlanItems_CategoryId",
                table: "SavingPlanItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingPlanItems_SavingPlanId",
                table: "SavingPlanItems",
                column: "SavingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingPlanMonthlyProgress_SavingPlanId_Year_Month",
                table: "SavingPlanMonthlyProgress",
                columns: new[] { "SavingPlanId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingPlans_AppUserId",
                table: "SavingPlans",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingPlans_AppUserId_Status",
                table: "SavingPlans",
                columns: new[] { "AppUserId", "Status" },
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavingPlanItems");

            migrationBuilder.DropTable(
                name: "SavingPlanMonthlyProgress");

            migrationBuilder.DropTable(
                name: "SavingPlans");
        }
    }
}
