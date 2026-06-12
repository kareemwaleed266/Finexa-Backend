using Finexa.Application.Common.Helpers;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Application.Modules.Bills.Interfaces;
using Finexa.Domain.Enums;

namespace Finexa.Api.BackgroundServices
{
    public class BillOccurrenceGenerationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BillOccurrenceGenerationBackgroundService> _logger;

        public BillOccurrenceGenerationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BillOccurrenceGenerationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunGenerationAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetDelayUntilNextRun();

                await Task.Delay(delay, stoppingToken);

                await RunGenerationAsync(stoppingToken);
            }
        }

        private async Task RunGenerationAsync(CancellationToken stoppingToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var billService = scope.ServiceProvider
                .GetRequiredService<IBillService>();

            var jobLogService = scope.ServiceProvider
                .GetRequiredService<IAdminJobLogService>();

            var jobLogId = await jobLogService.StartJobAsync(
                SystemJobName.BillOccurrenceGeneration,
                "System");

            try
            {
                var createdCount = await billService
                    .GenerateUpcomingOccurrencesForAllUsersAsync(90);

                await jobLogService.MarkJobSucceededAsync(
                    jobLogId,
                    processedCount: 0,
                    createdCount: createdCount);

                _logger.LogInformation(
                    "Bill occurrence generation finished. Created: {CreatedCount}",
                    createdCount);
            }
            catch (OperationCanceledException)
            {
                await jobLogService.MarkJobFailedAsync(
                    jobLogId,
                    "Bill occurrence generation was cancelled");

                _logger.LogWarning(
                    "Bill occurrence generation was cancelled");
            }
            catch (Exception ex)
            {
                await jobLogService.MarkJobFailedAsync(
                    jobLogId,
                    ex.Message);

                _logger.LogError(
                    ex,
                    "Error while generating upcoming bill occurrences");
            }
        }

        private static TimeSpan GetDelayUntilNextRun()
        {
            var egyptTimeZone = DateTimeHelper.GetEgyptTimeZone();

            var localNow = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                egyptTimeZone);

            var nextRun = localNow.Date.AddHours(2);

            if (nextRun <= localNow)
                nextRun = nextRun.AddDays(1);

            return nextRun - localNow;
        }
    }
}