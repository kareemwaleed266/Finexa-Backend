using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Finexa.Infrastructure.Security
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // 🔹 UserId
        public Guid UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?
                    .FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? _httpContextAccessor.HttpContext?.User?
                    .FindFirstValue("sub");

                if (string.IsNullOrWhiteSpace(userId))
                    return Guid.Empty;

                return Guid.TryParse(userId, out var parsedUserId)
                    ? parsedUserId
                    : Guid.Empty;
            }
        }

        // 🔹 Username
        public string? UserName =>
            _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.Name);

        // 🔹 Email
        public string? Email =>
            _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.Email)
            ?? _httpContextAccessor.HttpContext?.User?
                .FindFirstValue("email");

        // 🔥 Best option (جاهزة للاستخدام)
        public string? CurrentUserDisplayName =>
            UserName ?? Email ?? "System";
    }
}