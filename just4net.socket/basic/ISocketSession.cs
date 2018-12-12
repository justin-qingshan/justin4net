using just4net.socket.common;
using just4net.socket.protocol;
using System;
using System.Net;
using System.Net.Sockets;

namespace just4net.socket.basic
{
    public interface ISocketSession : ISessionBase
    {
        IAppSession AppSession { get; }

        Socket Client { get; }

        IPEndPoint LocalEndPoint { get; }

        int OriginRecOffset { get; }

        void Init(IAppSession appSession);

        void Start();

        void Close(CloseReason reason);

        bool TrySend(ArraySegment<byte> segment);

        Action<ISocketSession, CloseReason> Closed { get; set; }
    }

    public interface ISocketSession<TSocketSession, TRequestInfo>
        : ISocketSession
        where TSocketSession : ISocketSession
        where TRequestInfo : class, IRequestInfo
    {
        void Init(ISmartPool<SendingQueue> sendingQueuePool, IReceiveFilter<TRequestInfo> recFilter);
    }
}
