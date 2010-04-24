using System;
using System.Collections.Generic;

namespace Guanima.Redis.Commands.SortedSets
{
    [Serializable]
    public sealed class ZUnionCommand : BaseUnionIntersectionCommand
    {
        public ZUnionCommand(string key, IEnumerable<String> keys)
            : base(key, keys)
        {
        }

        public ZUnionCommand(string key, IEnumerable<string> keys, IEnumerable<double> weights, AggregateType aggregateType) :
            base(key, keys, weights, aggregateType)
        {
        }

        public override string Name
        {
            get
            {
                return "ZUNION";
            }
        }
    }
}
