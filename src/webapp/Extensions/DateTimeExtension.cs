namespace webapp.Extensions
{
    public static class DateTimeExtension
    {
        public static int GetEpocTime(this DateTime? time)
        {
            if (time is null) return 0;

            TimeSpan t = time.Value - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }
    }
}
