using System;
using Guanima.Redis.Extensions;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class SortCommand : RedisCommand
    {
        private string _str;

        public SortCommand(SortBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
           // Elements = builder.GetSortParameters();
            _str = builder.ToString();
        }

        // TODO: transition to new multi-bulk protocol
        public override void SendCommand(IRedisProtocol protocol)
        {
            var utf8 = (_str + "\r\n").ToUtf8ByteArray();
            protocol.Socket.Write(utf8);  
        }
    }

}
