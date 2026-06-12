using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] AdminUserFilterDto filter)
        {
            var users = await _adminUserService.GetUsersAsync(filter);

            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserDetails(Guid id)
        {
            var user = await _adminUserService.GetUserDetailsAsync(id);

            return Ok(user);
        }

        [HttpPost("{id:guid}/lock")]
        public async Task<IActionResult> LockUser(
            Guid id,
            [FromBody] AdminUserActionDto dto)
        {
            await _adminUserService.LockUserAsync(id, dto);

            return Ok(new
            {
                message = "User locked successfully"
            });
        }

        [HttpPost("{id:guid}/unlock")]
        public async Task<IActionResult> UnlockUser(
            Guid id,
            [FromBody] AdminUserActionDto dto)
        {
            await _adminUserService.UnlockUserAsync(id, dto);

            return Ok(new
            {
                message = "User unlocked successfully"
            });
        }

        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> ActivateUser(
            Guid id,
            [FromBody] AdminUserActionDto dto)
        {
            await _adminUserService.ActivateUserAsync(id, dto);

            return Ok(new
            {
                message = "User activated successfully"
            });
        }

        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> DeactivateUser(
            Guid id,
            [FromBody] AdminUserActionDto dto)
        {
            await _adminUserService.DeactivateUserAsync(id, dto);

            return Ok(new
            {
                message = "User deactivated successfully"
            });
        }

        [HttpPost("{id:guid}/make-admin")]
        public async Task<IActionResult> MakeAdmin(
            Guid id,
            [FromBody] AdminUserActionDto dto)
        {
            await _adminUserService.MakeAdminAsync(id, dto);

            return Ok(new
            {
                message = "User promoted to admin successfully"
            });
        }

        [HttpPost("{id:guid}/remove-admin")]
        public async Task<IActionResult> RemoveAdmin(
            Guid id,
            [FromBody] AdminUserActionDto dto)
        {
            await _adminUserService.RemoveAdminAsync(id, dto);

            return Ok(new
            {
                message = "Admin role removed successfully"
            });
        }
    }
}