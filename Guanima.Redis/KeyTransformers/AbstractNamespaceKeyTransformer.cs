using System;

namespace Guanima.Redis.KeyTransformers
{
	public abstract class AbstractNamespaceKeyTransformer : DefaultKeyTransformer
	{        
		public override string Transform(string key)
		{
			return Namespace + ":" + SafeKey(key);
		}

        public abstract String Namespace
        { 
            get;
        }
	}
}
