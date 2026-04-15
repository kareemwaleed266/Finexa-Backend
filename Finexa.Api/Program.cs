using Finexa.Api.Extensions;
using Finexa.Application;
using Finexa.Infrastructure;
using Finexa.Infrastructure.Security;
using Finexa.Integration;
using Finexa.Application.Interfaces.Persistence;
using System.Text.Json.Serialization;
using Finexa.Domain.Entities.Identity;
using Finexa.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;

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

            builder.Services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });
            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerDocumentation();

            builder.Services.AddCorsPolicy();
            builder.Services.AddGlobalMiddlewares();

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddIntegrationServices(builder.Configuration);

            #endregion

            var app = builder.Build();

            // Initialize Database
            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider
                    .GetRequiredService<IFinexaContextInitializer>();

                await initializer.InitializeAsync();
            }

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

                await RoleSeeder.SeedAsync(roleManager);
            }

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<FinexaDbContext>();

                await CategorySeeder.SeedAsync(context);
            }
            #region Configure Middlewares

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerDocumentation();
            }

            app.UseGlobalErrorHandler();

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            #endregion

            await app.RunAsync();
        }
    }
}