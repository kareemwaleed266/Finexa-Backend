using Finexa.Application.Common.DTOs;
using Finexa.Application.Common.Models;
using Finexa.Application.Modules.Goals.DTOs;

namespace Finexa.Application.Modules.Goals.Interfaces
{
    public interface IGoalService
    {
        Task CreateGoalAsync(CreateGoalDto dto);
        Task<PagedResult<GoalDto>> GetGoalsAsync(GoalFilterDto filter);
        Task ContributeToGoalAsync(Guid goalId, GoalContributionDto dto);
        Task<PagedResult<GoalHistoryDto>> GetGoalHistoryAsync(Guid goalId, BaseFilterDto filter);
        Task<GoalDetailsDto> GetGoalDetailsAsync(Guid goalId);
        Task CancelGoalAsync(Guid goalId);
        Task RefundGoalAsync(Guid goalId);
    }
}