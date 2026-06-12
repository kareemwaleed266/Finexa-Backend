using Finexa.Application.Common.Helpers;
using Finexa.Application.Modules.Notifications.Interfaces;

namespace Finexa.Api.BackgroundServices
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            await DelayUntilNextRunAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var scheduler = scope.ServiceProvider
                        .GetRequiredService<INotificationSchedulerService>();

                    await scheduler.RunDailyChecksAsync();

                    _logger.LogInformation(
                        "Notification daily checks completed at {Time}",
                        DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while running notification daily checks");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        private static async Task DelayUntilNextRunAsync(
            CancellationToken cancellationToken)
        {
            var egyptTimeZone = DateTimeHelper.GetEgyptTimeZone();

            var nowUtc = DateTime.UtcNow;

            var nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(
                nowUtc,
                egyptTimeZone);

            var nextRunEgypt = nowEgypt.Date.AddHours(8);

            if (nextRunEgypt <= nowEgypt)
                nextRunEgypt = nextRunEgypt.AddDays(1);

            var nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(
                nextRunEgypt,
                egyptTimeZone);

            var delay = nextRunUtc - nowUtc;

            await Task.Delay(delay, cancellationToken);
        }
    }
}