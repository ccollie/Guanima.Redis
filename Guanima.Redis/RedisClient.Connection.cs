using System;
using Guanima.Redis.Commands.Connection;

namespace Guanima.Redis
{
    public partial class RedisClient 
    {
        // ????
        public bool Ping()
        {
            bool alive = true;
            ForeachServer(node=>
                              {
                                  if (alive)
                                      alive = Ping(node);
                              });
            return alive;
        }

        public bool Ping(IRedisNode node)
        {
            if (!node.Ping())
                return false;
            try
            {
                string status = ExecValue(node, new PingCommand());
                return status == "pong";
            } 
            catch(RedisException)
            {
                return false;    
            }
        }

        
        public void Auth()
        {
            ForeachServer(node =>
            {
                if (!String.IsNullOrEmpty(node.Password))
                {
                    Execute(node, new AuthCommand(node.Password));
                }
            });
        }


        public void Auth(string password)
        {
            ForeachServer(node => Execute(node, new AuthCommand(node.Password)));
        }

        public void Auth(IRedisNode node, string password)
        {
            Execute(node, new AuthCommand(password));
        }

        public void Quit(IRedisNode node)
        {
            Execute(node, new QuitCommand());
        }

        public void Quit()
        {
            ForeachServer(node => Execute(node, new QuitCommand()));
        }
    }
}
