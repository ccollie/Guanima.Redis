using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands
{
    
    public abstract class RedisCommand : IRedisCommand
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(RedisCommand));
        internal const int OneGb = 1073741824;  // TODO: Move to global class

        private RedisValue[] _params;
        private RedisValue? _value;
        private String _name;
        private Func<RedisValue> _valueReader;

        protected RedisCommand()
        {
        }

        protected RedisCommand(params RedisValue[] parms)
        {
            SetParameters(parms);
        }

        protected RedisCommand(string name, params RedisValue[] parms)
        {
            Init(name, parms);
        }


        protected RedisValue[] Elements
        {
            get { return _params;}
            set { _params = value; }
        }

        protected void Init(string name, params RedisValue[] parameters)
        {
            var vals = new RedisValue[1 + parameters.Length];
            vals[0] = name;
            if (parameters.Length > 0)
                parameters.CopyTo(vals, 1);
            _params = vals;
        }

        protected void SetParameters(params RedisValue[] values)
        {
            // TODO: ensure not null
            Init(Name, values);
        }

        protected void ValidateKey(string key)
        {
            if (key == null) throw new ArgumentNullException("key", "Item key must be specified.");
            if (key.Length == 0) throw new ArgumentException("Item key must be specified.", "key");
        }
        #region SendCommand

        protected long DataLength(byte[] data)
        {
            return (data == null) ? 0 : data.Length;           
        }

        protected void ValidateDataLength(byte[] value)
        {
            if (DataLength(value) > OneGb)
                throw new ArgumentException("value exceeds 1G", "value");
        }

        #endregion

        #region IServerCommand

        public virtual string Name
        {
            get
            {
                if (String.IsNullOrEmpty(_name))
                {
                    _name = this.GetType().Name;
                    if (_name.EndsWith("Command"))
                        _name = _name.Substring(0, _name.Length - "Command".Length);
                    _name = _name.ToUpperInvariant();                
                }
                return _name;
            }
        }

        public virtual void SendCommand(IRedisProtocol protocol)
        {
            var p = Elements;
            if (p == null || p.Length == 0)
                protocol.IssueCommand(Name);
            else
            {
                var multi = new RedisValue { Type = RedisValueType.MultiBulk, MultiBulkValues = p };
                protocol.WriteValue(multi);
            }
        }


        public virtual void ReadReply(IRedisProtocol protocol)
        {
            this.Result = protocol.ReadReply();
            if (Result.Type == RedisValueType.Error)
            {
                throw new RedisException(Result.ErrorText);
            }
        }

        #endregion

        #region Misc

        protected string KeysToString(IEnumerable<String> keys)
        {
            var sb = new StringBuilder();
            foreach (var key in keys)
            {
                if (sb.Length > 0)
                    sb.Append(" ");

                sb.Append(key);
            }

            return sb.ToString();
        }

        #endregion

        public virtual RedisValue Execute(RedisProtocol protocol)
        {
            SendCommand(protocol);
            ReadReply(protocol);
            return Result;
        }

 
        public virtual RedisValue Result
        {
            get
            {
                return _value.HasValue ? _value.Value : RedisValue.Empty; // todo: have a null 
            }
            internal set
            {
                _value = value;
            }
        }

        public override string ToString()
        {
            if (_params == null)
                return base.ToString();
            var multi = new RedisValue { Type = RedisValueType.MultiBulk, MultiBulkValues = _params };
            return multi.ToString();
        }

        public static implicit operator RedisValue(RedisCommand command)
        {
            return command.Result;
        }
    }

    public abstract class ZeroArgsCommand : RedisCommand
    {
        public override void SendCommand(IRedisProtocol protocol)
        {
            protocol.IssueCommand(Name);
        }
    }

   
    public abstract class MultiKeyItemCommand : RedisCommand
    {
        private readonly List<String> _keys;

        protected MultiKeyItemCommand(IEnumerable<String> keys)
        {
            if (keys == null) throw new ArgumentNullException("keys", "Item key must be specified.");
            _keys = keys.ToList();
            if (_keys.Count == 0)
                throw new ArgumentException("At least one key must be specified.", "keys");

            Elements = CommandUtils.ConstructParameters(Name, null, keys);
        }


        public IList<String> Keys
        {
            get { return _keys; }
        }

        protected RedisValue[] KeysToParameters()
        {
            return CommandUtils.ConstructParameters(Keys);
        }

    }
}