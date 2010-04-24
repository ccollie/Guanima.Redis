using System;
using System.Text;

namespace Guanima.Redis.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToUtf8ByteArray(this String value)
        {
            return (value == null) ? null : Encoding.UTF8.GetBytes(value);
        }
    }
}
