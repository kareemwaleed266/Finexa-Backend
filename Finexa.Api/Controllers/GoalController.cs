using Finexa.Application.Modules.Goals.DTOs;
using Finexa.Application.Modules.Goals.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class GoalController : ControllerBase
    {
        private readonly IGoalService _goalService;

        public GoalController(IGoalService goalService)
        {
            _goalService = goalService;
        }

        [HttpPost("create-goal")]
        public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDto dto)
        {
            await _goalService.CreateGoalAsync(dto);

            return Ok(new { message = "Goal created successfully" });
        }

        [HttpGet("get-goals")]
        public async Task<IActionResult> GetGoals()
        {
            var result = await _goalService.GetGoalsAsync();

            return Ok(result);
        }

        [HttpPost("{id}/contribute")]
        public async Task<IActionResult> ContributeToGoal(Guid id, [FromBody] GoalContributionDto dto)
        {
            await _goalService.ContributeToGoalAsync(id, dto);

            return Ok(new { message = "Contribution added successfully" });
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetGoalHistory(Guid id)
        {
            var result = await _goalService.GetGoalHistoryAsync(id);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGoalDetails(Guid id)
        {
            var result = await _goalService.GetGoalDetailsAsync(id);

            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelGoal(Guid id)
        {
            await _goalService.CancelGoalAsync(id);

            return Ok(new { message = "Goal canceled successfully" });
        }

        [HttpPost("{id}/refund")]
        public async Task<IActionResult> RefundGoal(Guid id)
        {
            await _goalService.RefundGoalAsync(id);

            return Ok(new { message = "Refund completed successfully" });
        }

    }
}