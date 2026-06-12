using Finexa.Application.Modules.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers.Admin
{
    [Route("api/admin/system-health")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminSystemHealthController : ControllerBase
    {
        private readonly IAdminSystemHealthService _adminSystemHealthService;

        public AdminSystemHealthController(IAdminSystemHealthService adminSystemHealthService)
        {
            _adminSystemHealthService = adminSystemHealthService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var health = await _adminSystemHealthService.GetHealthAsync();

            return Ok(health);
        }
    }
}