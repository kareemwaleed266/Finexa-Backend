using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers.Admin
{
    [Route("api/admin/jobs")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminJobsController : ControllerBase
    {
        private readonly IAdminJobLogService _adminJobLogService;

        public AdminJobsController(IAdminJobLogService adminJobLogService)
        {
            _adminJobLogService = adminJobLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobLogs([FromQuery] SystemJobLogFilterDto filter)
        {
            var logs = await _adminJobLogService.GetLogsAsync(filter);

            return Ok(logs);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestJob([FromQuery] SystemJobName? jobName)
        {
            var latestJob = await _adminJobLogService.GetLatestAsync(jobName);

            return Ok(latestJob);
        }
    }
}