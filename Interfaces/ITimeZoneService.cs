namespace TransactionAPI.Interfaces
{
    public interface ITimeZoneService
    {
        string GetIanaTimeZone(double latitude, double longitude);
    }
}
