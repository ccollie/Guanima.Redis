using System;

namespace Guanima.Redis.Extensions
{
    public static class DateTimeExtensions
    {
        public const long UnixEpoch = 621355968000000000L;


        public static readonly DateTime UnixEpochDateTime = new DateTime(1970, 1, 1);

        public static DateTime AsDateTime(this long value)
        {
            return UnixEpochDateTime.AddSeconds(value);    
        }

        public static long UnixTimeNow
        {
            get
            {
                // TODO: Have a testable datetime provider
                var now = DateTime.UtcNow;
                return now.ToUnixTimestamp();
            }
        }
        /// <summary>
        /// Convert DateTime to Unix time stamp
        /// </summary>
        /// <param name="dateTime">datetime to convert</param>
        /// <returns>Return Unix time stamp as long type</returns>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            TimeSpan unixTimespan = (dateTime.ToUniversalTime() - UnixEpochDateTime);
            return (long)unixTimespan.TotalSeconds;
        }
    }
}
