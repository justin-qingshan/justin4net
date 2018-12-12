using just4net.socket.basic;

namespace just4net.socket.protocol
{
    public interface IRequestHandler<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        void HandleRequest(IAppSession session, TRequestInfo request);
    }
}
