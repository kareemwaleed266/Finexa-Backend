using Finexa.Application.Modules.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers.Admin
{
    [Route("api/admin/ai")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminAiMonitoringController : ControllerBase
    {
        private readonly IAdminAiMonitoringService _adminAiMonitoringService;

        public AdminAiMonitoringController(IAdminAiMonitoringService adminAiMonitoringService)
        {
            _adminAiMonitoringService = adminAiMonitoringService;
        }

        [HttpGet("usage-summary")]
        public async Task<IActionResult> GetUsageSummary()
        {
            var summary = await _adminAiMonitoringService.GetUsageSummaryAsync();

            return Ok(summary);
        }
    }
}