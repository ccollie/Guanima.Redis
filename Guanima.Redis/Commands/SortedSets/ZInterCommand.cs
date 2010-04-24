using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZInterCommand : BaseUnionIntersectionCommand
    {
        public ZInterCommand(string key, IEnumerable<String> keys)
            : base(key, keys)
        {
        }

        public ZInterCommand(string key, IEnumerable<string> keys, IEnumerable<double> weights, AggregateType aggregateType) :
            base(key, keys, weights, aggregateType)
        {
        }
    }
}
