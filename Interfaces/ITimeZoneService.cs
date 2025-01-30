namespace TransactionAPI.Interfaces
{
    public interface ITimeZoneService
    {
        string GetIanaTimeZoneFromLocation(string location);
        DateTime ConvertToTimeZone(DateTime dateTime, string sourceIana, string targetIana);
    }
}
