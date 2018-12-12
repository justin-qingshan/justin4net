using just4net.socket.protocol;
using System;
using System.Net;
using System.Text;

namespace just4net.socket.basic
{
    public interface IAppSession : ISessionBase
    {
        IAppServer AppServer { get; }

        ISocketSession SocketSession { get; }

        IServerConfig Config { get; }

        IPEndPoint LocalEndPoint { get; }

        DateTime LastActiveTime { get; set; }

        DateTime StartTime { get; }

        void Close();

        void Close(CloseReason reason);

        bool Connected { get; }

        Encoding Charset { get; set; }

        ILog Logger { get; }

        int ProcessRequest(byte[] readBuffer, int offset, int length, bool toBeCopied);

        void StartSession();
    }

    public interface IAppSession<TAppSession, TRequestInfo> : IAppSession
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
    {
        void Init(IAppServer<TAppSession, TRequestInfo> server, ISocketSession session);
    }
}
