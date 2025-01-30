using GeoTimeZone;
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

        public DateTime ConvertToTimeZone(DateTime dateTime, string sourceIana, string targetIana)
        {
            TimeZoneInfo sourceZone = TZConvert.GetTimeZoneInfo(sourceIana);
            TimeZoneInfo targetZone = TZConvert.GetTimeZoneInfo(targetIana);

            return TimeZoneInfo.ConvertTime(dateTime, sourceZone, targetZone);
        }
    }
}
