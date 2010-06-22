using System;
using System.Collections.Generic;
using System.Text;
using Guanima.Redis.Protocol;

namespace Guanima.Redis.Commands.SortedSets
{
    //todo: move elsewhere
    public enum AggregateType
    {
        None,
        Sum,
        Min,
        Max,
        Avg
    }

    public class BaseUnionIntersectionCommand : KeyCommand
    {
        private readonly List<String> _keys;
        private readonly List<Double> _weights;
        private readonly AggregateType _aggregate = AggregateType.None;

        public BaseUnionIntersectionCommand(string key, IEnumerable<String> keys)
            :this(key, keys, null, AggregateType.None)
        {
                    
        }

        public BaseUnionIntersectionCommand(string key, IEnumerable<String> keys,
            IEnumerable<Double> weights, AggregateType aggregateType) 
            : base(key)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");
            _keys = new List<String>(keys);
            if (_keys.Count < 2)
                throw new CommandArgumentException("keys", "At least 2 keys must be specified");
            if (weights != null)
            {
                _weights = new List<Double>(_weights);
                if (_weights.Count != _keys.Count)
                {
                    throw new CommandArgumentException("weights",
                        "The number of weights must match the number of keys.");
                }
            }
            _aggregate = aggregateType;
        }


        protected string WeightsToString()
        {
            var sb = new StringBuilder();
            foreach (var weight in _weights)
            {
                if (sb.Length > 0)
                    sb.Append(" ");

                sb.Append(weight);
            }

            return sb.ToString();
        }

        public override void WriteTo(PooledSocket socket)
        {
            var sb = new StringBuilder()
                .Append(" ")
                .Append(_keys.Count)
                .Append(" ")
                .Append(KeysToString(_keys));

            if (_weights != null && _weights.Count>0)
            {
                sb.Append(" ");
                sb.Append(KeysToString(_keys));    
            }
            if (_aggregate != AggregateType.None)
            {
                var aggr = "";
                switch (_aggregate)
                {
                    case AggregateType.Min:
                        aggr = "MIN"; 
                        break;
                    case AggregateType.Max:
                        aggr = "MAX";
                        break;
                    case AggregateType.Sum:
                        aggr = "SUM";
                        break;
                }
                if (!String.IsNullOrEmpty(aggr))
                {
                    sb.Append(" ")
                        .Append("AGGREGATE ")
                        .Append(aggr);
                }
            }
            //protocol.IssueSimpleCommand(Name, sb.ToString());
            throw new NotImplementedException();
        }
    }
}
