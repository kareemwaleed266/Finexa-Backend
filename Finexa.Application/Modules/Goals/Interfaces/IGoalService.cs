using Finexa.Application.Modules.Goals.DTOs;

namespace Finexa.Application.Modules.Goals.Interfaces
{
    public interface IGoalService
    {
        Task CreateGoalAsync(CreateGoalDto dto);
        Task<List<GoalDto>> GetGoalsAsync();
        Task ContributeToGoalAsync(Guid goalId, GoalContributionDto dto);
        Task<List<GoalHistoryDto>> GetGoalHistoryAsync(Guid goalId);
        Task<GoalDetailsDto> GetGoalDetailsAsync(Guid goalId);
        Task CancelGoalAsync(Guid goalId);
        Task RefundGoalAsync(Guid goalId);
    }
}