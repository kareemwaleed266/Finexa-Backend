using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.SavingPlans.DTOs
{
    public class GenerateSavingPlanRequestDto
    {
        public int Months { get; set; } = 3;

        public SavingPlanType PlanType { get; set; } = SavingPlanType.Balanced;

        public decimal? TargetMonthlySaving { get; set; }
    }
}