using Finexa.Domain.Enums;

namespace Finexa.Application.Common.Helpers
{
    public static class DateRangeHelper
    {
        public static (DateTime from, DateTime to) GetRange(
            PeriodType? period,
            DateTime? from,
            DateTime? to)
        {
            var egyptTimeZone = DateTimeHelper.GetEgyptTimeZone();

            var utcNow = DateTime.UtcNow;

            var localNow = TimeZoneInfo.ConvertTimeFromUtc(
                utcNow,
                egyptTimeZone);

            DateTime localFrom;
            DateTime localTo;

            if (period == PeriodType.All)
            {
                return (DateTime.MinValue, DateTime.UtcNow);
            }

            if (period == PeriodType.Custom || from.HasValue || to.HasValue)
            {
                localFrom = from?.Date
                    ?? new DateTime(localNow.Year, localNow.Month, 1);

                localTo = to.HasValue
                    ? to.Value.Date.AddDays(1).AddTicks(-1)
                    : localNow;

                return (
                    DateTimeHelper.ConvertClientLocalToUtc(localFrom),
                    DateTimeHelper.ConvertClientLocalToUtc(localTo)
                );
            }

            switch (period)
            {
                case PeriodType.Today:
                    localFrom = localNow.Date;
                    localTo = localNow;
                    break;

                case PeriodType.Week:
                    localFrom = localNow.Date.AddDays(
                        -(int)((7 + (localNow.DayOfWeek - DayOfWeek.Saturday)) % 7));
                    localTo = localNow;
                    break;

                case PeriodType.Month:
                    localFrom = new DateTime(localNow.Year, localNow.Month, 1);
                    localTo = localNow;
                    break;

                case PeriodType.Year:
                    localFrom = new DateTime(localNow.Year, 1, 1);
                    localTo = localNow;
                    break;

                default:
                    localFrom = new DateTime(localNow.Year, localNow.Month, 1);
                    localTo = localNow;
                    break;
            }

            return (
                DateTimeHelper.ConvertClientLocalToUtc(localFrom),
                DateTimeHelper.ConvertClientLocalToUtc(localTo)
            );
        }
    }
}