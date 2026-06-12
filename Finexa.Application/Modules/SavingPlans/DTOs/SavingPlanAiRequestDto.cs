using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.SavingPlans.DTOs
{
    public class SavingPlanAiRequestDto
    {
        public int Months { get; set; }

        public SavingPlanType PlanType { get; set; }

        public decimal? TargetMonthlySaving { get; set; }

        //public string Currency { get; set; } = "EGP";

        public List<MonthlySummaryDto> MonthlySummary { get; set; } = new();

        public List<CategorySummaryDto> CategorySummary { get; set; } = new();
    }
}