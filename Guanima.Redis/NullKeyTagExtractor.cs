namespace Guanima.Redis
{
    public class NullKeyTagExtractor : IKeyTagExtractor
    {
        public string GetKeyTag(string key)
        {
            return key;
        }
    }
}
