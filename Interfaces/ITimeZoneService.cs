namespace TransactionAPI.Interfaces
{
    public interface ITimeZoneService
    {
        string GetIanaTimeZoneAsync(double latitude, double longitude);
    }
}
