using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.AI.Chat.Interfaces;
using Finexa.Application.Modules.AI.Chat.Services;
using Finexa.Application.Modules.AI.STT.Interfaces;
using Finexa.Application.Modules.Categories.Interfaces;
using Finexa.Application.Modules.Categories.Services;
using Finexa.Application.Modules.Dashboard.Interfaces;
using Finexa.Application.Modules.Goals.Interfaces;
using Finexa.Application.Modules.Goals.Services;
using Finexa.Application.Modules.Identity.Interfaces;
using Finexa.Application.Modules.Identity.Services;
using Finexa.Application.Modules.Transactions.Interfaces;
using Finexa.Application.Modules.Transactions.Services;
using Finexa.Domain.Entities.Identity;
using Finexa.Infrastructure.Persistence.Interceptors;
using Finexa.Infrastructure.Persistence.Repositories;
using Finexa.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Finexa.Application.Modules.AI.STT.Services;
using Finexa.Application.Modules.AI.ParseTransaction.Interfaces;
using Finexa.Application.Modules.AI.ParseTransaction.Services;
using Finexa.Application.Modules.AI.OCR.Interfaces;
using Finexa.Application.Modules.AI.OCR.Services;
using Finexa.Application.Modules.Email.Interfaces;
using Finexa.Application.Modules.Email;
using Finexa.Application.Settings;
using Finexa.Application.Common.Settings;
using Finexa.Application.Modules.Bills.Interfaces;
using Finexa.Application.Modules.Bills.Services;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Application.Modules.Admin.Services;
using Finexa.Application.Modules.SavingPlans.Interfaces;
using Finexa.Application.Modules.SavingPlans.Services;
using Finexa.Application.Modules.Notifications.Interfaces;
using Finexa.Application.Modules.Notifications.Services;

namespace Finexa.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 🔹 Interceptors
            services.AddScoped<AuditableEntityInterceptor>();

            // 🔹 DbContext
            services.AddDbContext<FinexaDbContext>((provider, options) =>
            {
                var audit = provider.GetRequiredService<AuditableEntityInterceptor>();

                options
                    .UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(
                                typeof(FinexaDbContext).Assembly.FullName);
                        })
                    .AddInterceptors(audit);
            });

            // 🔹 Identity
            services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<FinexaDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            // 🔹 Repositories
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 🔹 Database Initializer
            services.AddScoped<IFinexaContextInitializer, FinexaContextInitializer>();

            // 🔹 Security
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();

            services.Configure<FrontendSettings>(configuration.GetSection("FrontendSettings"));

            // 🔹 Other services can be registered here...
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IGoalService, GoalService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            // AI Services
            services.AddScoped<ISpeechAppService, SpeechAppService>();
            services.AddScoped<IChatAppService, ChatAppService>();
            services.AddScoped<IParseTransactionAppService, ParseTransactionAppService>();
            services.AddScoped<IAiTransactionMapperService, AiTransactionMapperService>();
            services.AddScoped<IOcrAppService, OcrAppService>();


            // Bills services
            services.AddScoped<IBillService, BillService>();

            // Admin services
            services.AddScoped<IAdminAuditLogService, AdminAuditLogService>();
            services.AddScoped<IAdminJobLogService, AdminJobLogService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminCategoryService, AdminCategoryService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IAdminBillsMonitoringService, AdminBillsMonitoringService>();
            services.AddScoped<IAdminAiMonitoringService, AdminAiMonitoringService>();
            services.AddScoped<IAdminSystemHealthService, AdminSystemHealthService>();
            services.Configure<AdminSeedSettings>(configuration.GetSection("AdminSeed"));

            // saving plan services
            services.AddScoped<ISavingPlanService, SavingPlanService>();

            // Notification services
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationSchedulerService, NotificationSchedulerService>();

            return services;
        }
    }
}