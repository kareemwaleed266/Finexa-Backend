using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Modules.Identity.DTOs;
using Microsoft.AspNetCore.Http;

namespace Finexa.Application.Modules.Identity.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<AuthResponseDto> RefreshAsync();
        Task LogoutAsync();
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task ResendConfirmationLinkAsync(string email);
    }
}
