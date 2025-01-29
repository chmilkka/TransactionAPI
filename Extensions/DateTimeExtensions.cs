namespace TransactionAPI.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToDatabaseDateTimeFormat(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
