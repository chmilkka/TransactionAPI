using GeoTimeZone;
using NodaTime;
using System.Globalization;
using TimeZoneConverter;
using TransactionAPI.Interfaces;

namespace TransactionAPI.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        public string GetIanaTimeZoneFromLocation(string location)
        {
            var locationParts = location.Split(',');

            double latitude = double.Parse(locationParts[0], CultureInfo.InvariantCulture);
            double longitude = double.Parse(locationParts[1], CultureInfo.InvariantCulture);

            var timeZone = TimeZoneLookup.GetTimeZone(latitude, longitude);

            return timeZone.Result ?? throw new Exception("Failed to get time zone.");
        }           

        public DateTime ConvertToUtc(DateTime dateTime, string timeZoneId)
        {
            var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

            var localDateTime = new LocalDateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

            var zonedDateTime = timeZone.AtLeniently(localDateTime);

            var utcDateTime = DateTime.SpecifyKind(zonedDateTime.ToInstant().ToDateTimeUtc(), DateTimeKind.Utc);

            return utcDateTime;
        }
    }
}
