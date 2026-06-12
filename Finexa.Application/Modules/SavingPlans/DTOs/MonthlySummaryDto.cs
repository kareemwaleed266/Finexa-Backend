namespace Finexa.Application.Modules.SavingPlans.DTOs
{
    public class MonthlySummaryDto
    {
        public string Month { get; set; } = string.Empty;

        public decimal Income { get; set; }

        public decimal Expenses { get; set; }

        public decimal Saving { get; set; }
    }
}