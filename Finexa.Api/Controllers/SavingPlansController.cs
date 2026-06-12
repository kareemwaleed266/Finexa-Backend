using Finexa.Application.Modules.SavingPlans.DTOs;
using Finexa.Application.Modules.SavingPlans.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers
{
    [ApiController]
    [Route("api/saving-plans")]
    [Authorize(Roles = "User")]
    public class SavingPlansController : ControllerBase
    {
        private readonly ISavingPlanService _savingPlanService;

        public SavingPlansController(ISavingPlanService savingPlanService)
        {
            _savingPlanService = savingPlanService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePreview(
            [FromBody] GenerateSavingPlanRequestDto request)
        {
            var result = await _savingPlanService.GeneratePreviewAsync(request);

            return Ok(result);
        }

        [HttpPost("{id:guid}/apply")]
        public async Task<IActionResult> Apply(Guid id)
        {
            var result = await _savingPlanService.ApplyAsync(id);

            return Ok(new
            {
                message = "Saving plan applied successfully",
                plan = result
            });
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePlan()
        {
            var result = await _savingPlanService.GetActivePlanAsync();

            if (result == null)
            {
                return NotFound(new
                {
                    message = "No active saving plan found"
                });
            }

            return Ok(result);
        }

        [HttpGet("active/progress")]
        public async Task<IActionResult> GetActiveProgress()
        {
            var result = await _savingPlanService.GetActiveProgressAsync();

            return Ok(result);
        }

        [HttpGet("active/monthly-progress")]
        public async Task<IActionResult> GetActiveMonthlyProgress()
        {
            var result = await _savingPlanService.GetActiveMonthlyProgressAsync();

            return Ok(result);
        }

        [HttpPost("active/monthly-progress/sync")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SyncActiveMonthlyProgress()
        {
            await _savingPlanService.SyncActiveMonthlyProgressAsync();

            return Ok(new
            {
                message = "Saving plan monthly progress synced successfully"
            });
        }

        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await _savingPlanService.DeactivateAsync(id);

            return Ok(new
            {
                message = "Saving plan deactivated successfully"
            });
        }
    }
}