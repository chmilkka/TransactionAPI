using GeoTimeZone;
using System.Text.Json.Serialization;
using TransactionAPI.Interfaces;

namespace TransactionAPI.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        public string GetIanaTimeZoneAsync(double latitude, double longitude)
        {

            var timeZone = TimeZoneLookup.GetTimeZone(latitude, longitude);

            return timeZone.Result ?? throw new Exception("Failed to get time zone.");
        }
    }
}
