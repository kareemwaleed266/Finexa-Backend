using Finexa.Application.Modules.SavingPlans.DTOs;

namespace Finexa.Application.Modules.SavingPlans.Interfaces
{
    public interface ISavingPlanAiService
    {
        Task<SavingPlanResponseDto> GenerateAsync(SavingPlanAiRequestDto request);
    }
}