using System;

namespace Guanima.Redis.Commands.Hashes
{
    public abstract class HashFieldCommand : RedisCommand
    {
        protected HashFieldCommand(string key, string field, params RedisValue[] parms) 
        {
            ValidateKey(key);
            ValidateField(field);
            RedisValue[] vals = new RedisValue[3 + parms.Length];
            vals[0] = Name;
            vals[1] = key;
            vals[2] = field;
            if (parms.Length > 0)
                parms.CopyTo(vals,3);
            Elements = vals;
        }

        protected void ValidateField(string field)
        {
            if (field == null)
                throw new ArgumentNullException("field");
            if (string.IsNullOrEmpty(field))
                throw new ArgumentException("Empty field name passed to " + Name);

        }

     }
}