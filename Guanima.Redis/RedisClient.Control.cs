using System;
using System.Collections.Generic;
using Guanima.Redis.Commands.Control;
using Guanima.Redis.Commands.Persistence;
using Guanima.Redis.Extensions;

namespace Guanima.Redis
{
    public partial class RedisClient
    {
        // TODO: For each of these, provide overrides to function on a particular node

        public DateTime LastSave()
        {
            var command = new LastSaveCommand();
            long timestamp = 0;
            ForeachServer(node=>
                              {
                                var temp = ExecuteInt(node, command);
                                timestamp = Math.Max(temp, timestamp);
                              });

            return  timestamp.AsDateTime();
        }

        public void Save()
        {
            ForeachServer(new SaveCommand());
        }

        public void BgSave()
        {
            ForeachServer(new BGSaveCommand());
        }

        public void BgRewriteAof()
        {
            ForeachServer(new BGRewriteAOFCommand());
        }

        public void Shutdown()
        {
            ForeachServer(new ShutdownCommand());
        }

        private Dictionary<String,RedisValue> GetInfo(IRedisNode node)
        {
            String info = ExecValue(node, new InfoCommand());
            string[] data = info.Split(new string[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            var infos = new Dictionary<string, RedisValue>();
            foreach (var item in data)
            {
                var spec = item.Split(':');
                infos.Add(spec[0], spec[1]);
            }
            return infos;
        }

        public Dictionary<string,RedisValue> Info()
        {
            // TODO: Crap out if more than one server in pool
            Dictionary<string, RedisValue> info = null;
            ForeachServer(
                node => { if (info == null) info = GetInfo(node); }
                );
            return info;
        }

        public Dictionary<string,RedisValue> Info(string alias)
        {
            var node = GetNodeByAlias(alias);
            return GetInfo(node);
        }
    }
}
