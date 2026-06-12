using Finexa.Application.Modules.SavingPlans.DTOs;

namespace Finexa.Application.Modules.SavingPlans.Interfaces
{
    public interface ISavingPlanService
    {
        Task<SavingPlanResponseDto> GeneratePreviewAsync(
            GenerateSavingPlanRequestDto request);

        Task<SavingPlanResponseDto> ApplyAsync(Guid draftPlanId);
        Task<SavingPlanResponseDto?> GetActivePlanAsync();

        Task<SavingPlanCurrentProgressDto> GetActiveProgressAsync();

        Task<List<SavingPlanMonthlyProgressDto>> GetActiveMonthlyProgressAsync();

        Task SyncActiveMonthlyProgressAsync();

        Task DeactivateAsync(Guid savingPlanId);
    }
}