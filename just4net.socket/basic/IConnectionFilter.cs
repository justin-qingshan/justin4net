using System.Net;

namespace just4net.socket.basic
{
    public interface IConnectionFilter
    {
        bool Init(IAppServer appServer);

        bool AllowConnect(IPEndPoint remoteEndPoint);
    }
}
