using just4net.socket.basic;
using just4net.socket.protocol;
using just4net.socket.server;

namespace just4net.socket.engine
{
    public class SocketServerFactory : ISocketServerFactory
    {
        public ISocketServer CreateSocketServer<TRequestInfo>(IAppServer appServer, ListenerInfo[] listeners, IServerConfig config) where TRequestInfo : IRequestInfo
        {
            return new AsyncSocketServer(appServer, listeners);
        }
    }
}
