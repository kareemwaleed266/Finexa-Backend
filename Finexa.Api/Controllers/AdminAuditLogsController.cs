using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers.Admin
{
    [Route("api/admin/audit-logs")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminAuditLogsController : ControllerBase
    {
        private readonly IAdminAuditLogService _adminAuditLogService;

        public AdminAuditLogsController(IAdminAuditLogService adminAuditLogService)
        {
            _adminAuditLogService = adminAuditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AdminAuditLogFilterDto filter)
        {
            var logs = await _adminAuditLogService.GetLogsAsync(filter);

            return Ok(logs);
        }
    }
}