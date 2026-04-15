using Microsoft.AspNetCore.Http;
using Finexa.Application.Modules.Identity.DTOs;

namespace Finexa.Application.Modules.Identity.Interfaces
{
    public interface IUserService
    {
        Task UpdateProfileAsync(UpdateProfileDto dto);

        Task<string> UploadProfileImageAsync(IFormFile file);

        Task ChangePasswordAsync(ChangePasswordDto dto);

        Task DeleteAccountAsync();

        Task<object> GetProfileAsync();
    }
}