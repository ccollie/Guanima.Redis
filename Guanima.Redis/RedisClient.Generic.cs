using System;
using System.Collections.Generic;
using Guanima.Redis.Commands.Generic;
using Guanima.Redis.Commands.Strings;

namespace Guanima.Redis
{
    public partial class RedisClient
    {
        private int _currentDb = 0;

        protected int CurrentDB
        {
            get { return _currentDb; }
        }

        public int Del(params String[] keys)
        {
            return Del((IEnumerable<string>)keys);
        }

    
        public int Del(IEnumerable<String> keys)
        {
            int deleteCount = 0;
            var transformed = TransformKeys(keys);

            IDictionary<IRedisNode, List<string>> splitKeys = SplitKeys(transformed);

            // send a 'DEL' to each server
            foreach (var de in splitKeys)
            {
                var command = new DelCommand(de.Value);
                deleteCount += ExecuteInt(de.Key, command);
            }

            return deleteCount;
        }


        public bool Exists(string key)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new ExistsCommand(transformedKey));
        }

        public bool Expire(string key, TimeSpan timeout)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new ExpireCommand(transformedKey, timeout));
        }

        public bool Expire(string key, int timeoutInSeconds)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new ExpireCommand(transformedKey, timeoutInSeconds));
        }


        public bool ExpireAt(string key, DateTime when)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new ExpireAtCommand(transformedKey, when));
        }

        public bool ExpireAt(string key, int unixTime)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new ExpireAtCommand(transformedKey, unixTime));
        }

        public int DBSize()
        {
            int result = 0;
            ForeachServer(node => result += ExecuteInt(node, new DBSizeCommand()));
            return result;
        }

        public int DBSize(IRedisNode node)
        {
            return ExecuteInt(node, new DBSizeCommand());
        }

        public RedisValue Echo(IRedisNode node, RedisValue value)
        {
            return ExecValue(node, new EchoCommand(value));    
        }

        public RedisValue Echo(RedisValue value)
        {
            // TODO: need to figure out the semantic of this in cluster mode....
            // Maybe error out if more than 1 server ?
            int count = 0;
            var result = RedisValue.Empty; 
            ForeachServer(node=>
                              {
                                  if (count == 0)
                                  {
                                      result = ExecValue(node, new EchoCommand(value));
                                      count++;
                                  }
                              });
            return result;
        }


        public RedisClient FlushAll()
        {
            ForeachServer(node => Execute(node, new FlushAllCommand()));
            return this;
        }

        public RedisClient FlushAll(IRedisNode node)
        {
            Execute(node, new FlushAllCommand());
            return this;
        }

        public RedisClient FlushDB()
        {
            // TODO: cluster command
            ForeachServer(node => Execute(node, new FlushDBCommand()));
            return this;
        }


        public bool Move(string key, int db)
        {
            var transformedKey = TransformKey(key);
            return ExecuteBool(transformedKey, new MoveCommand(transformedKey, db));
        }

        public bool Rename(string oldName, string newName)
        {
            // Note. We need to consider how this interacts with sharding.
            var transformedOldName = TransformKey(oldName);
            var transformedNewName = TransformKey(newName);

            // i.e. if the new node is different from the old.
            IRedisNode oldNode = GetNodeForTransformedKey(transformedOldName);
            IRedisNode newNode = GetNodeForTransformedKey(transformedNewName);

            // if they're the same, no problem, just do a regular rename
            if (oldNode == newNode)
            {
                Execute(oldNode, new RenameCommand(transformedOldName, transformedNewName));
                return true;
            }

            CannotCluster("RENAME");
            return false;
        }

        public bool RenameNX(string oldName, string newName)
        {
            // Note. We need to consider how this interacts with sharding.

            var transformedOldName = TransformKey(oldName);
            var transformedNewName = TransformKey(newName);

            // i.e. if the new node is different from the old.
            IRedisNode oldNode = GetNodeForTransformedKey(transformedOldName);
            IRedisNode newNode = GetNodeForTransformedKey(transformedNewName);

            // if they're the same, no problem, just do a regular rename
            if (oldNode == newNode)
            {
                var renameCommand = new RenameNXCommand(transformedOldName, transformedNewName);
                return ExecuteInt(oldNode, renameCommand) > 0;
            }

            CannotCluster("RENAMENX");
            return false;
        }

 
        public RedisClient Select(int db)
        {
            // TODO: Needs major thinking through. Since we're using pooled sockets,
            // we need to track the current db per socket, which means that when
            // they're acquired from the pool, an implicit (to the user) Select
            // has to be made to ensure we dont hose whatever DB the last client
            // set it to. 
            // Thinking about it, in the pooled context, it is useless on its
            // own short of pipelining, since for a single SELECT single command, a socket
            // is acquired, selected and placed directly back into the pool,
            // and whatever actions we want to perform next have to grab
            // sockets with possibly different DBs set and incur the overhead
            // of an additional call/roundtrip per command. I wonder if the
            // Ruby lib's decision to freeze the DB at construction has anything
            // to do with this.
            // The moral of the story (currently) is that if you want to use
            // select, do it as part of a pipleline/transaction, or you end up
            // incurring extra/unnecessary overhead.
            if (!Pipelining)
            {
                // raise warning
            }
            ForeachServer(node=>Execute(node, new SelectCommand(db)));
            _currentDb = db;            
            return this;
        }


        public RedisValue Sort(string key, string by, bool alpha, int? start, int? count)
        {
            return Sort(key, by, alpha, false, start, count);
        }

        public RedisValue Sort(string key, bool alpha, int? start, int? count)
        {
            return Sort(key, null, alpha, false, start, count);
        }

        public RedisValue Sort(string key, bool alpha, bool descending, int? start, int? count)
        {
            return Sort(key, null, alpha, descending, start, count);
        }

        public RedisValue Sort(string key, string by, bool alpha, bool descending, int? start, int? count)
        {
            var transformedKey = TransformKey(key);

            var sb = new SortBuilder(transformedKey)
                .Alpha(alpha)
                .Desc(descending);

            if (!String.IsNullOrEmpty(by))
                sb.By(by);
            
            if (start.HasValue && count.HasValue)
                sb.Limit(start.Value, count.Value);


            var command = new SortCommand(sb);
            return ExecValue(key, command);
        }

        public RedisValue Sort(SortBuilder sb)
        {
            // todo: clone first
            if (!String.IsNullOrEmpty(sb.ResultKey))
            {
                sb.StoreTo(TransformKey(sb.ResultKey));
            }
            if (!String.IsNullOrEmpty(sb.Key))
            {
                sb.Key = TransformKey(sb.Key);
            }
            // TODO: need to transform patterns as well
            return ExecValue(sb.Key, new SortCommand(sb));
        }


        public TimeSpan Ttl(string key)
        {
            var transformedKey = TransformKey(key);
            var temp = ExecuteInt(transformedKey, new TtlCommand(transformedKey));
            return TimeSpan.FromSeconds(temp);
        }

        public RedisType Type(string key)
        {
            var transformedKey = TransformKey(key);
            string typeString = ExecValue(transformedKey, new TypeCommand(transformedKey));
            switch (typeString.ToLower())
            {
                case "none":
                    return RedisType.None;
                case "string":
                    return RedisType.String;
                case "list":
                    return RedisType.List;
                case "set":
                    return RedisType.Set;
                case "zset":
                case "sortedset":
                    return RedisType.SortedSet;
                case "hash":
                    return RedisType.Hash;
                default:
                    throw new RedisClientException("Unknown type");
            }
        }
    }
}
