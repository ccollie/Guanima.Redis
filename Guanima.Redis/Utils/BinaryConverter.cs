using System;
using System.Text;

namespace Guanima.Redis.Utils
{
    internal static class BinaryConverter
    {

        public static byte[] EncodeKey(string key)
        {
            if (String.IsNullOrEmpty(key)) return null;

            return Encoding.UTF8.GetBytes(key);
        }

        public static string DecodeKey(byte[] data)
        {
            if (data == null || data.Length == 0) return null;

            return Encoding.UTF8.GetString(data);
        }

        public static string DecodeKey(byte[] data, int index, int count)
        {
            if (data == null || data.Length == 0 || count == 0) return null;

            return Encoding.UTF8.GetString(data, index, count);
        }
    }
}

#region [ License information          ]
/* ************************************************************
 *
 * Copyright (c) Attila Kiskó, enyim.com
 *
 * This source code is subject to terms and conditions of 
 * Microsoft Permissive License (Ms-PL).
 * 
 * A copy of the license can be found in the License.html
 * file at the root of this distribution. If you can not 
 * locate the License, please send an email to a@enyim.com
 * 
 * By using this source code in any fashion, you are 
 * agreeing to be bound by the terms of the Microsoft 
 * Permissive License.
 *
 * You must not remove this notice, or any other, from this
 * software.
 *
 * ************************************************************/
#endregion