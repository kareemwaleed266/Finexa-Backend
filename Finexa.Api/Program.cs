using System.Text.Json.Serialization;
using Finexa.Api.BackgroundServices;
using Finexa.Api.Extensions;
using Finexa.Api.Hubs;
using Finexa.Api.Realtime;
using Finexa.Application;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Notifications.Interfaces;
using Finexa.Domain.Entities.Identity;
using Finexa.Infrastructure;
using Finexa.Infrastructure.Persistence.Seed;
using Finexa.Infrastructure.Security;
using Finexa.Integration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Finexa.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure Services

            builder.Host.ConfigureSerilog(builder.Configuration);

            builder.Services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters
                    .Add(new JsonStringEnumConverter());
            });


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerDocumentation();

            builder.Services.AddCorsPolicy();
            builder.Services.AddGlobalMiddlewares();

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddIntegrationServices(builder.Configuration);
            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.Services.AddHostedService<BillOccurrenceGenerationBackgroundService>();

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });

            builder.Services.AddSignalR();

            builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();

            builder.Services.AddScoped<INotificationRealtimeSender, SignalRNotificationSender>();
            builder.Services.AddHostedService<NotificationBackgroundService>();
            #endregion

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider
                    .GetRequiredService<IFinexaContextInitializer>();

                await initializer.InitializeAsync();

                var roleManager = scope.ServiceProvider
                    .GetRequiredService<RoleManager<AppRole>>();

                await RoleSeeder.SeedAsync(roleManager);

                var context = scope.ServiceProvider
                    .GetRequiredService<FinexaDbContext>();

                await CategorySeeder.SeedAsync(context);

                await AdminSeeder.SeedAsync(scope.ServiceProvider);
            }

            #region Configure Middlewares

            app.UseSwaggerDocumentation();

            app.UseGlobalErrorHandler();

            app.UseHttpsRedirection();

            app.UseCors("Frontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notifications");

            #endregion

            await app.RunAsync();
        }
    }
}