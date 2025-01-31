namespace TransactionAPI.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToDatabaseDateTimeFormat(this DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                throw new ArgumentException("Invalid DateTime value.");
            }

            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
