namespace Guanima.Redis.Commands.SortedSets
{
    public class ZRevRangeByScoreCommand : BaseZRangeByScoreCommand
    {
        public ZRevRangeByScoreCommand(string key, double min, double max)
            : base(key, min, max)
        {
        }

        public ZRevRangeByScoreCommand(string key, double min, double max, int offset, int count)
            : base(key, min, max, offset, count)
        {
        }

        public ZRevRangeByScoreCommand(string key, double start, double end, bool withScores)
            : base(key, start, end, withScores)
        {
        }

        public ZRevRangeByScoreCommand(string key, int start, int end, int offset, int count, bool withScores)
            : base(key, start, end, offset, count, withScores)
        {
        }
    }
}
