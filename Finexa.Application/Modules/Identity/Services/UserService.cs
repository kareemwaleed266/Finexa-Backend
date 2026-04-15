using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Identity.DTOs;
using Finexa.Application.Modules.Identity.Interfaces;
using Finexa.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Finexa.Application.Modules.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserService _currentUser;

        public UserService(
            UserManager<AppUser> userManager,
            ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _currentUser = currentUser;
        }

        private async Task<AppUser> GetCurrentUserAsync()
        {
            var userId = _currentUser.UserId;

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new Exception("User not found");

            return user;
        }

        public async Task UpdateProfileAsync(UpdateProfileDto dto)
        {
            var user = await GetCurrentUserAsync();

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.DateOfBirth = dto.DateOfBirth;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                await _userManager.SetPhoneNumberAsync(user, dto.PhoneNumber);

            await _userManager.UpdateAsync(user);
        }

        public async Task<string> UploadProfileImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Invalid file");

            var allowedTypes = new[] { "image/jpeg", "image/png" };

            if (!allowedTypes.Contains(file.ContentType))
                throw new Exception("Only JPG and PNG are allowed");

            if (file.Length > 2 * 1024 * 1024) // 2MB
                throw new Exception("File size must be less than 2MB");

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var folder = Path.Combine("wwwroot", "images");

            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var user = await GetCurrentUserAsync();

            user.ProfileImageUrl = $"/images/{fileName}";

            await _userManager.UpdateAsync(user);

            return user.ProfileImageUrl!;
        }

        public async Task ChangePasswordAsync(ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new Exception("Passwords do not match");

            var user = await GetCurrentUserAsync();

            var result = await _userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword
            );

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task DeleteAccountAsync()
        {
            var user = await GetCurrentUserAsync();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception("Failed to delete account");
        }

        public async Task<object> GetProfileAsync()
        {
            var user = await GetCurrentUserAsync();

            return new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.DateOfBirth,
                user.ProfileImageUrl
            };
        }
    }
}