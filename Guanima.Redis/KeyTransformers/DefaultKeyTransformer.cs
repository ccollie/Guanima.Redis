namespace Guanima.Redis.KeyTransformers
{
	public class DefaultKeyTransformer : KeyTransformerBase
	{
        protected static string SafeKey(string key)
        {
            return key == null ? null : key.Replace(' ', '_')
                    .Replace('\t', '_').Replace('\n', '_');
        }

		public override string Transform(string key)
		{
			return SafeKey(key);
		}
	}
}
