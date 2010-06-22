using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Guanima.Redis.Client;

namespace Guanima.Redis.Commands
{
    
    public abstract class RedisCommand : IRedisCommand
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(RedisCommand));
        internal const int OneGb = 1073741824;  // TODO: Move to global class

        private RedisValue[] _params;
        private RedisValue? _value;
        private String _name;

        protected RedisCommand()
        {
            Init(Name);
        }

        protected RedisCommand(params RedisValue[] parms)
        {
            SetParameters(parms);
        }

        protected RedisCommand(string name, params RedisValue[] parms)
        {
            Init(name, parms);
        }


        internal RedisValue[] Arguments
        {
            get { return _params;}
            set { _params = value; }
        }

        internal RedisCommandQueue Queue { get; set; }
        internal Action<RedisCommand> ValueReader { get; set; }

        protected void Init(byte[] name, params RedisValue[] parameters)
        {
            var vals = new RedisValue[1 + parameters.Length];
            vals[0] = name;
            if (parameters.Length > 0)
                parameters.CopyTo(vals, 1);
            _params = vals;
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

        public virtual void WriteTo(PooledSocket socket)
        {
            var p = Arguments;
            if (p == null || p.Length == 0)
            {
                RedisValue nameVal = Name;
                p = new RedisValue[1] { nameVal };
            }
            var multi = new RedisValue { Type = RedisValueType.MultiBulk, MultiBulkValues = p };
            multi.Write(socket);
        }


        public virtual void ReadFrom(PooledSocket socket)
        {
            socket.FlushSendBuffer();
            this.Value = RedisValue.Read(socket);
            if (Value.Type == RedisValueType.Error)
            {
                throw new RedisException(Value.ErrorText);
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

        public virtual RedisValue Execute(PooledSocket socket)
        {
            WriteTo(socket);
            ReadFrom(socket);
            return Value;
        }

 
        public virtual RedisValue Value
        {
            get
            {
                if (!_value.HasValue)
                {
                    if (Queue != null)
                        Queue.ReadResultForCommand(this);
                }
                return _value.Value;
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
            return command.Value;
        }
    }

    public abstract class ZeroArgsCommand : RedisCommand
    {
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

            Arguments = CommandUtils.ConstructParameters(Name, null, keys);
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