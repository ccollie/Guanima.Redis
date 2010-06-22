using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.PubSub
{
    public class UnsubscribeCommand : SubscribeCommand
    {
        public UnsubscribeCommand()
        {
            base.Init(Command.UnSubscribe);            
        }

        public UnsubscribeCommand(IEnumerable<string> channels)
        {
            if (channels == null)
                throw new ArgumentNullException("channels");
            Arguments = CommandUtils.ConstructParameters(Command.UnSubscribe, channels);
        }
    }
}
