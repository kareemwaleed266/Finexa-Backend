using Finexa.Application.Modules.Identity.DTOs;
using Finexa.Application.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _userService.GetProfileAsync();
            return Ok(result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            await _userService.UpdateProfileAsync(dto);
            return Ok("Profile updated successfully");
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var url = await _userService.UploadProfileImageAsync(file);
            return Ok(new { imageUrl = url });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            await _userService.ChangePasswordAsync(dto);
            return Ok("Password changed successfully");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            await _userService.DeleteAccountAsync();
            return Ok("Account deleted successfully");
        }
    }
}