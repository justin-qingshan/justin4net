using just4net.socket.protocol;

namespace just4net.socket.basic
{
    public interface ISocketServerFactory
    {
        ISocketServer CreateSocketServer<TRequestInfo>(IAppServer appServer, ListenerInfo[] listeners, IServerConfig config)
            where TRequestInfo : IRequestInfo;
    }
}
