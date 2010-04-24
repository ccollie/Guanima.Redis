namespace Guanima.Redis.KeyTransformers
{
	public class NamespaceKeyTransformer : AbstractNamespaceKeyTransformer
	{
	    private readonly string _namespace;
        
        public NamespaceKeyTransformer(string nspace)
        {
            if (string.IsNullOrEmpty(nspace))
                throw new RedisClientException("Namespace cannot be null or empty");
            _namespace = nspace;
        }


        public override string Namespace
        { 
            get { return _namespace;}
        }
	}
}
