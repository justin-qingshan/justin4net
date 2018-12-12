using just4net.socket.basic;
using System.Net;

namespace just4net.socket.protocol
{
    public interface IReceiveFilterFactory<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        IReceiveFilter<TRequestInfo> CreateFilter(IAppServer server, IAppSession session, IPEndPoint remoteEndPoint);
    }
}
