using System.Collections.Generic;

namespace Guanima.Redis.Configuration
{
	public class AuthenticationConfiguration : IAuthenticationConfiguration
	{
	    private Dictionary<string, object> _parameters;

	    public string Password
        {
            get; set;
        }

		Dictionary<string, object> IAuthenticationConfiguration.Parameters
		{
			get { return _parameters ?? (_parameters = new Dictionary<string, object>()); }
		}
	}
}
