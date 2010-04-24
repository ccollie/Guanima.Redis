using System.Net;

namespace Guanima.Redis.Configuration
{
    public interface IEndPointConfiguration
    {
        IPEndPoint EndPoint { get;}
        string Alias { get; }
        string Password { get; }
    }
}