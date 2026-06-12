using System.Globalization;

namespace Finexa.Application.Common.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ConvertClientLocalToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;

            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();

            var egyptTimeZone = GetEgyptTimeZone();

            var unspecifiedLocalTime = DateTime.SpecifyKind(
                value,
                DateTimeKind.Unspecified
            );

            return TimeZoneInfo.ConvertTimeToUtc(
                unspecifiedLocalTime,
                egyptTimeZone
            );
        }

        public static DateTime EnsureUtcKind(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public static DateTime? EnsureUtcKind(DateTime? value)
        {
            return value.HasValue
                ? EnsureUtcKind(value.Value)
                : null;
        }

        public static TimeZoneInfo GetEgyptTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            }
        }

        public static DateTime? ParseClientLocalDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                return null;
            }

            return DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
        }
    }
}