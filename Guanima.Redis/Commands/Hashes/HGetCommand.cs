namespace Guanima.Redis.Commands.Hashes
{
    public class HGetCommand : HashFieldCommand
    {
        public HGetCommand(string key, string field)
            : base(key, field)
        {
           
        }
    }
}
