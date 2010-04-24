namespace Guanima.Redis.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Get the array slice between the two indexes, inclusive.
        /// </summary>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends
            if (end < 0)
            {
                if (end > start)
                {            
                    start = source.Length + start;
                    end = source.Length + end;
                }
                else
                {
                    end = source.Length - start - end - 1;
                }
            }
            int len = end - start + 1;

            // Return new array
            var res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }

    }
}
