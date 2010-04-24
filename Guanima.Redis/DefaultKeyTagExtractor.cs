using System;

namespace Guanima.Redis
{
    public class DefaultKeyTagExtractor : IKeyTagExtractor
    {
        public string GetKeyTag(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("Key values cannot be empty.");
            var start = key.IndexOf("{");
            if (start < 0)
                return key;
            var end = key.IndexOf("}");
            if (end < 0)
                throw new ArgumentException("Missing '}' in key.","key");
            var length = end - start - 1;
            return key.Substring(start + 1, length);
        }
    }
}
