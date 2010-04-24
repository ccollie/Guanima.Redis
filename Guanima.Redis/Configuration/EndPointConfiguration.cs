using System.Net;

namespace Guanima.Redis.Configuration
{
    // TODO: Change name to ServerConfiguration ?
    public class EndPointConfiguration : IEndPointConfiguration
    {
        public EndPointConfiguration() 
            :this(new IPEndPoint(IPAddress.Loopback, 6379), null, null)
        {
            
        }

        public EndPointConfiguration(IPEndPoint endPoint)
            :this(endPoint, null, null)
        {
            
        }

        public EndPointConfiguration(IPEndPoint endPoint, string alias, string password)
        {
            EndPoint = endPoint;
            Alias = alias ?? endPoint.ToString();
            Password = password;
        }

        public IPEndPoint EndPoint
        {
            get; private set;
        }

        public string Alias { get; private set; }

        public string Password { get; private set; }
    }
}
