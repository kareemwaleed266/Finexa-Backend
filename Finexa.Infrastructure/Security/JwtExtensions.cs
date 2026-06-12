using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Finexa.Infrastructure.Security
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>();

            if (jwtSettings is null ||
                string.IsNullOrWhiteSpace(jwtSettings.Secret) ||
                string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
                string.IsNullOrWhiteSpace(jwtSettings.Audience))
            {
                throw new Exception("JwtSettings is missing or invalid in configuration.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.Secret)),

                        ClockSkew = TimeSpan.Zero,

                        NameClaimType = ClaimTypes.NameIdentifier,
                        RoleClaimType = ClaimTypes.Role
                    };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal?.Identity
                            as ClaimsIdentity;

                        if (identity == null)
                            return Task.CompletedTask;

                        var sub = identity.FindFirst("sub")?.Value;

                        if (!string.IsNullOrWhiteSpace(sub) &&
                            identity.FindFirst(ClaimTypes.NameIdentifier) == null)
                        {
                            identity.AddClaim(
                                new Claim(ClaimTypes.NameIdentifier, sub));
                        }

                        return Task.CompletedTask;
                    },

                    OnMessageReceived = context =>
                    {
                        var accessToken =
                            context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrWhiteSpace(accessToken) &&
                            path.StartsWithSegments("/hubs/notifications"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}