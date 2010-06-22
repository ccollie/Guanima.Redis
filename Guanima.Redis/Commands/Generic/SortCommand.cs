using System;

namespace Guanima.Redis.Commands.Generic
{
    [Serializable]
    public sealed class SortCommand : RedisCommand
    {
        public SortCommand(SortBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            this.SetParameters(builder.GetSortParameters());
        }
    }

}
