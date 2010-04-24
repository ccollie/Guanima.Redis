using System;

namespace Guanima.Redis.Extensions
{
    public static class IntExtensions
    {

        public static TimeSpan Days(this int value)
        {
            return TimeSpan.FromDays(value);
        }
        
        public static TimeSpan Hours(this int value)
        {
            return TimeSpan.FromHours(value);
        }

        public static TimeSpan Minutes(this int value)
        {
            return TimeSpan.FromMinutes(value);
        }

        public static TimeSpan Seconds(this int value)
        {
            return TimeSpan.FromSeconds(value);
        }


        public static void Times(this int value, Action<int> action)
        {
            for (int i = 0; i < value; i++)
                action(i);
        }

        public static bool IsInRange(this long value, long min, long max)
        {
            if (min > max)
                throw new ArgumentOutOfRangeException("min", "min must be less than or equal to max");
            return (value >= min && value <= max);
        }

        public static bool IsBetween(this long value, long min, long max)
        {
            if (min > max)
            {
                var temp = min;
                min = max;
                max = temp;
            }
            return (value >= min && value <= max);
        }
    }
}
