using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Guanima.Redis.Extensions
{
    public static class ByteArrayExtensions
    {
        public static bool IsEqualTo(this byte[] data1, byte[] data2)
        {
            // If both are null, they're equal
            if (data1 == null && data2 == null)
            {
                return true;
            }
            // If either but not both are null, they're not equal
            if (data1 == null || data2 == null)
            {
                return false;
            }
            if (data1.Length != data2.Length)
            {
                return false;
            }
            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != data2[i])
                {
                    return false;
                }
            }
            return true;
        }

 
        public static string AsString(this byte[] value)
        {
            if (value == null) return null;
            if (value.Length == 0)
                return string.Empty;
            return Encoding.UTF8.GetString(value);
        }

        public static List<String> ToStringList(this byte[][] value)
        {
            var result = new List<String>();
            if (value != null && value.Length > 0)
            {
                foreach (var v in value)
                {
                    result.Add(Encoding.UTF8.GetString(v));
                }
            }
            return result;
        }

        public static HashSet<string> ToHashSet(this byte[][] multiDataList)
        {
            var results = new HashSet<string>();
            foreach (var multiData in multiDataList)
            {
                results.Add(multiData.AsString());
            }
            return results;
        }

        public static IDictionary<string, byte[]> AsAlternatingItemDictionary(this byte[][] multiDataList)
        {
            var dict = new Dictionary<string, byte[]>();
            for (var i = 0; i < multiDataList.Length; i += 2)
            {
                dict[multiDataList[i].AsString()] = multiDataList[i + 1];
            }

            return dict;
        }
    }
}
