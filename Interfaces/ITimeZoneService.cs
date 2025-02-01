namespace TransactionAPI.Interfaces
{
    public interface ITimeZoneService
    {
        string GetIanaTimeZoneFromLocation(string location);
        DateTime ConvertToUtc(DateTime dateTime, string timeZoneId);
    }
}
