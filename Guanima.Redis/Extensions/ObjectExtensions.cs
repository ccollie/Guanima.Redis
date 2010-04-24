using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Guanima.Redis.Extensions
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this object o)
        {
            var result = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            if (o == null)
                return result;
            if (o is IDictionary)
            {
                foreach (DictionaryEntry entry in (IDictionary)o)
                    result.Add(entry.Key.ToString(), entry.Value);
            }
            else
            {
                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(o))
                    result.Add(prop.Name, prop.GetValue(o));
            }
            return result;
        }
    }
}
