using Finexa.Application.Modules.Identity.DTOs;
using Finexa.Application.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailDto dto)
    {
        await _authService.ConfirmEmailAsync(dto);
        return Ok(new { message = "Email confirmed successfully" });
    }

    [HttpPost("resend-confirmation-link")]
    public async Task<IActionResult> ResendConfirmationLinkAsync(string email)
    {
        await _authService.ResendConfirmationLinkAsync(email);

        return Ok(new { message = "A new confirmation link has been sent to your email."});
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto);
        return Ok(new { message = "If email exists, reset link sent" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok(new { message = "Password reset successfully" });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh()
    {
        var result = await _authService.RefreshAsync();
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { message = "Logged out successfully" });
    }
}



