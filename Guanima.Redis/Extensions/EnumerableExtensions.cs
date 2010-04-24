using System;
using System.Collections.Generic;
using System.Text;

namespace Guanima.Redis.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null || action==null)
                return;

            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static string Join(this IEnumerable<String> keys)
        {
            return Join(keys, " ");
        }

        public static string Join(this IEnumerable<String> keys, string separator)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
            {
                if (sb.Length > 0)
                    sb.Append(separator);

                sb.Append(key);
            }

            return sb.ToString();
        }

    }
}
