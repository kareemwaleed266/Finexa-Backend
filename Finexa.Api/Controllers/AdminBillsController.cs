using Finexa.Application.Modules.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers.Admin
{
    [Route("api/admin/bills")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminBillsController : ControllerBase
    {
        private readonly IAdminBillsMonitoringService _adminBillsMonitoringService;

        public AdminBillsController(IAdminBillsMonitoringService adminBillsMonitoringService)
        {
            _adminBillsMonitoringService = adminBillsMonitoringService;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _adminBillsMonitoringService.GetOverviewAsync();

            return Ok(overview);
        }
    }
}