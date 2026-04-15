using Finexa.Application.Modules.Identity.DTOs;
using Finexa.Application.Modules.Identity.Interfaces;
using Finexa.Domain.Entities.Identity;
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

        public AuthService(
           UserManager<AppUser> userManager,
           SignInManager<AppUser> signInManager,
           IJwtTokenGenerator jwtTokenGenerator,
           IUnitOfWork unitOfWork,
           IOptions<JwtSettings> jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtOptions.Value;
        }


        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
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
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(appUser, dto.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

            await _userManager.AddToRoleAsync(appUser, "User");

            var balance = new UserBalance
            {
                Id = Guid.NewGuid(),
                AppUserId = appUser.Id,
                TotalIncome = 0,
                TotalExpense = 0,
                TotalBalance = 0
            };

            await _unitOfWork.Repository<UserBalance, Guid>()
                .AddAsync(balance);

            await _unitOfWork.SaveChangesAsync();


            var token = await _jwtTokenGenerator.GenerateTokenAsync(appUser);

            return new AuthResponseDto
            {
                Email = appUser.Email!,
                Username = appUser.UserName!,
                Token = token,
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes)
            };
        }


        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var email = dto.Email;

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new Exception("Invalid email or password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
                throw new Exception("Invalid email or password");

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = await _jwtTokenGenerator.GenerateTokenAsync(user);

            return new AuthResponseDto
            {
                Email = user.Email!,
                Username = user.UserName!,
                Token = token,
                ExpireAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes)
            };
        }
    }
}