using Finexa.Application.Common.Files;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Identity.DTOs;
using Finexa.Application.Modules.Identity.Interfaces;
using Finexa.Domain.Entities.Ai.Chat;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        public UserService(
            UserManager<AppUser> userManager,
            ICurrentUserService currentUser, IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _currentUser = currentUser;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
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

            var allowedTypes = new[]
                    {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/webp"
                     };

            if (string.IsNullOrWhiteSpace(file.ContentType) ||
                !allowedTypes.Contains(file.ContentType.ToLower()))
            {
                throw new Exception("Only JPG, PNG, and WEBP images are allowed");
            }

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File size must be less than 5MB");

            var user = await GetCurrentUserAsync();

            if (!string.IsNullOrWhiteSpace(user.ProfileImagePublicId))
            {
                await _fileStorageService.DeleteAsync(user.ProfileImagePublicId);
            }

            FileUploadResultDto uploadedFile;

            await using (var stream = file.OpenReadStream())
            {
                uploadedFile = await _fileStorageService.UploadImageAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    "finexa/profiles");
            }

            user.ProfileImageUrl = uploadedFile.Url;
            user.ProfileImagePublicId = uploadedFile.PublicId;
            user.LastModifiedAt = DateTime.UtcNow;
            user.LastModifiedBy = user.Email;

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
            var userId = user.Id;

            var chatSessionRepo = _unitOfWork.Repository<ChatSession, Guid>();
            var chatMessageRepo = _unitOfWork.Repository<ChatMessage, Guid>();

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
            var attachmentRepo = _unitOfWork.Repository<TransactionAttachment, Guid>();

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();
            var userBalanceRepo = _unitOfWork.Repository<UserBalance, Guid>();
            var categoryRepo = _unitOfWork.Repository<Category, Guid>();
            var refreshTokenRepo = _unitOfWork.Repository<RefreshToken, Guid>();

            var chatSessionIds = await chatSessionRepo.Query()
                .Where(s => s.AppUserId == userId)
                .Select(s => s.Id)
                .ToListAsync();

            var chatMessages = await chatMessageRepo.Query(withTracking: true)
                .Where(m => chatSessionIds.Contains(m.SessionId))
                .ToListAsync();

            foreach (var message in chatMessages)
                chatMessageRepo.Delete(message);

            var chatSessions = await chatSessionRepo.Query(withTracking: true)
                .Where(s => s.AppUserId == userId)
                .ToListAsync();

            foreach (var session in chatSessions)
                chatSessionRepo.Delete(session);

            var transactionIds = await transactionRepo.Query()
                .Where(t => t.AppUserId == userId)
                .Select(t => t.Id)
                .ToListAsync();

            var attachments = await attachmentRepo.Query(withTracking: true)
                .Where(a => transactionIds.Contains(a.TransactionId))
                .ToListAsync();

            foreach (var attachment in attachments)
                attachmentRepo.Delete(attachment);

            var transactions = await transactionRepo.Query(withTracking: true)
                .Where(t => t.AppUserId == userId)
                .ToListAsync();

            foreach (var transaction in transactions)
                transactionRepo.Delete(transaction);

            var goals = await goalRepo.Query(withTracking: true)
                .Where(g => g.AppUserId == userId)
                .ToListAsync();

            foreach (var goal in goals)
                goalRepo.Delete(goal);

            var userBalances = await userBalanceRepo.Query(withTracking: true)
                .Where(b => b.AppUserId == userId)
                .ToListAsync();

            foreach (var balance in userBalances)
                userBalanceRepo.Delete(balance);

            var categories = await categoryRepo.Query(withTracking: true)
                .Where(c => c.AppUserId == userId)
                .ToListAsync();

            foreach (var category in categories)
                categoryRepo.Delete(category);

            var refreshTokens = await refreshTokenRepo.Query(withTracking: true)
                .Where(r => r.AppUserId == userId)
                .ToListAsync();

            foreach (var token in refreshTokens)
                refreshTokenRepo.Delete(token);

            await _unitOfWork.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
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