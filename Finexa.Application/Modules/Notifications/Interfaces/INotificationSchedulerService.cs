namespace Finexa.Application.Modules.Notifications.Interfaces
{
    public interface INotificationSchedulerService
    {
        Task RunDailyChecksAsync();
    }
}