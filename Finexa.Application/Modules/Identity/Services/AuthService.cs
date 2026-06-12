using System.Security.Cryptography;
using Finexa.Application.Common.Settings;
using Finexa.Application.Modules.Email;
using Finexa.Application.Modules.Email.Interfaces;
using Finexa.Application.Modules.Identity.DTOs;
using Finexa.Application.Modules.Identity.Interfaces;
using Finexa.Domain.Entities.Ai.Chat;
using Finexa.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Finexa.Application.Modules.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly FrontendSettings _frontendSettings;
        public AuthService(
           UserManager<AppUser> userManager,
           SignInManager<AppUser> signInManager,
           IJwtTokenGenerator jwtTokenGenerator,
           IUnitOfWork unitOfWork,
           IOptions<JwtSettings> jwtOptions,
           IHttpContextAccessor httpContextAccessor,
           IEmailService emailService,
           IOptions<FrontendSettings> frontendOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _frontendSettings = frontendOptions.Value;
        }


        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            var email = dto.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                throw new Exception("Email already exists");

            if (dto.Password != dto.ConfirmPassword)
                throw new Exception("Passwords do not match");

            var appUser = new AppUser
            {
                Email = email,
                UserName = dto.Username.Trim(),
                EmailConfirmed = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = email
            };

            var result = await _userManager.CreateAsync(appUser, dto.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

            var addToRoleResult = await _userManager.AddToRoleAsync(appUser, "User");

            if (!addToRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addToRoleResult.Errors.Select(x => x.Description));
                throw new Exception(errors);
            }

            var balance = new UserBalance
            {
                Id = Guid.NewGuid(),
                AppUserId = appUser.Id,
                TotalIncome = 0,
                TotalExpense = 0,
                TotalBalance = 0
            };

            await _unitOfWork.Repository<UserBalance, Guid>().AddAsync(balance);
            await _unitOfWork.SaveChangesAsync();

            await GenerateEmailConfirmationLinkAsync(appUser);

            return "Registration successful. Please check your email to confirm your account.";
        }


        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var email = dto.Email;

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new Exception("Invalid email or password");

            if (!user.EmailConfirmed)
                throw new Exception("Please confirm your email first");


            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
                throw new Exception("Invalid email or password");

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
                throw new UnauthorizedAccessException("Your account is locked");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account is deactivated");

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = await _jwtTokenGenerator.GenerateTokenAsync(user);

            var repo = _unitOfWork.Repository<RefreshToken, Guid>();

            var existingTokens = await repo
                .WhereAsync(x => x.AppUserId == user.Id && !x.IsRevoked);

            foreach (var t in existingTokens)
            {
                t.IsRevoked = true;
            }

            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                AppUserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await repo.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            _httpContextAccessor.HttpContext!.Response.Cookies.Append(
                "refreshToken",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

            var chatRepo = _unitOfWork.Repository<ChatSession, Guid>();

            var existingSession = (await chatRepo
                .WhereAsync(x => x.AppUserId == user.Id))
                .FirstOrDefault();

            if (existingSession == null)
            {
                var newSession = new ChatSession
                {
                    Id = Guid.NewGuid(),
                    AppUserId = user.Id,
                    Title = user.UserName,
                    LastActivityAt = DateTime.UtcNow
                };

                await chatRepo.AddAsync(newSession);

                await _unitOfWork.SaveChangesAsync();
            }

            return new AuthResponseDto
            {
                Email = user.Email!,
                Username = user.UserName!,
                Token = token,
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes)
            };
        }

        public async Task ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());

            if (user == null)
                throw new Exception("Invalid user.");

            var decodedToken = Uri.UnescapeDataString(dto.Token);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                throw new Exception("Invalid or expired token.");
        }

        public async Task ResendConfirmationLinkAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new Exception("Invalid email");
            }

            await GenerateEmailConfirmationLinkAsync(user);

        }
        public async Task<AuthResponseDto> RefreshAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var refreshToken = httpContext?.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException("Invalid refresh token");

            var repo = _unitOfWork.Repository<RefreshToken, Guid>();

            var token = await repo.FirstOrDefaultAsync(x =>
                x.Token == refreshToken);

            if (token == null)
                throw new UnauthorizedAccessException("Token not found");

            if (token.IsRevoked)
                throw new UnauthorizedAccessException("Token revoked");

            if (token.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Token expired");

            var user = await _userManager.FindByIdAsync(token.AppUserId.ToString());

            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account is deactivated");

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
                throw new UnauthorizedAccessException("Your account is locked");

            token.IsRevoked = true;

            var newRefreshToken = GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = newRefreshToken,
                AppUserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await repo.AddAsync(newRefreshTokenEntity);

            var accessToken = await _jwtTokenGenerator.GenerateTokenAsync(user);

            await _unitOfWork.SaveChangesAsync();

            httpContext!.Response.Cookies.Append(
                "refreshToken",
                newRefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

            return new AuthResponseDto
            {
                Email = user.Email!,
                Username = user.UserName!,
                Token = accessToken,
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes)
            };
        }
        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(token);

            var resetLink = $"{_frontendSettings.BaseUrl}/reset-password?email={email}&token={encodedToken}";

            var body = $@"
                        <div style='font-family:Arial,sans-serif'>
                            <h2>Password Reset Request</h2>
                            <p>You requested to reset your password.</p>
                            <p>Click the button below to continue:</p>

                            <a href='{resetLink}' 
                               style='
                                 display:inline-block;
                                 padding:10px 20px;
                                 background-color:#000;
                                 color:#fff;
                                 text-decoration:none;
                                 border-radius:5px;
                               '>
                               Reset Password
                            </a>

                            <p style='margin-top:20px;font-size:12px;color:gray'>
                                If you did not request this, please ignore this email.
                            </p>
                        </div>
                        ";

            await _emailService.SendEmailAsync(
                email,
                "Reset Your Password",
                body
            );
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token))
                throw new ArgumentException("Invalid request");

            var email = dto.Email.Trim().ToLower();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new ArgumentException("Invalid request");

            var decodedToken = Uri.UnescapeDataString(dto.Token);

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                dto.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException(errors);
            }

            await _userManager.UpdateSecurityStampAsync(user);
        }
        public async Task LogoutAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var refreshToken = httpContext?.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return;

            var repo = _unitOfWork.Repository<RefreshToken, Guid>();

            var token = await repo.FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                await _unitOfWork.SaveChangesAsync();
            }

            httpContext!.Response.Cookies.Delete("refreshToken");
        }
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
        private async Task GenerateEmailConfirmationLinkAsync(AppUser user)
        {

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = $"{_frontendSettings.BaseUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailService.SendEmailAsync(
                user.Email!,
                 "Confirm Your Email",
                 $@"
           <div style='font-family:Arial,sans-serif;line-height:1.6'>
               <h2>Welcome to Finexa 👋</h2>

               <p>Thank you for creating an account.</p>
               <p>Please confirm your email to activate your account.</p>

               <a href='{confirmationLink}' 
                  style='
                    display:inline-block;
                    padding:12px 24px;
                    background-color:#0d6efd;
                    color:#ffffff;
                    text-decoration:none;
                    border-radius:6px;
                    font-weight:bold;
                    margin-top:15px;
                  '>
                  Confirm Email
               </a>

               <p style='margin-top:20px;font-size:13px;color:gray'>
                   If you did not create this account, you can safely ignore this email.
               </p>

               <hr style='margin-top:25px;border:none;border-top:1px solid #eee'>

               <p style='font-size:12px;color:#999'>
                   Finexa Team
               </p>
           </div>
           "
            );
        }
    }
}