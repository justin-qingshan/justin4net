using System.Net;

namespace just4net.socket.engine
{
    public interface ISession
    {
        string SessionID { get; }

        IPEndPoint RemoteEndPoint { get; }
    }
}
