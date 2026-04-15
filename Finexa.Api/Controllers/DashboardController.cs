using Finexa.Application.Modules.Dashboard.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _dashboardService.GetDashboardAsync();

            return Ok(result);
        }

        [HttpPost("rebuild-balance")]
        public async Task<IActionResult> RebuildBalance()
        {
            await _dashboardService.RebuildBalanceAsync();

            return Ok(new { message = "Balance rebuilt successfully" });
        }
    }
}