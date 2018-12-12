using just4net.socket.protocol;
using System;
using System.Collections.Generic;

namespace just4net.socket.basic
{
    public delegate void SessionHandler<TAppSession>(TAppSession session)
        where TAppSession : IAppSession;

    public delegate void SessionHandler<TAppSession, TParam>(TAppSession session, TParam value)
        where TAppSession : IAppSession;

    public delegate void RequestHandler<TAppSession, TRequestInfo>(TAppSession session, TRequestInfo request)
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IRequestInfo;

    public interface IAppServer : ILoggerProvider
    {
        DateTime StartTime { get; }

        IServerConfig Config { get; }

        ListenerInfo[] Listeners { get; }

        ISocketServer SocketServer { get; }

        int SessionCount { get; }

        ServerState State { get; }

        bool Start();

        void Stop();
        
        IAppSession CreateAppSession(ISocketSession socketSession);

        bool RegisterSession(IAppSession session);

        IAppSession GetSessionByID(int sessionId);
    }

    public interface IAppServer<TAppSession> : IAppServer
        where TAppSession : IAppSession
    {
        IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera);

        IEnumerable<TAppSession> GetAllSession();

        event SessionHandler<TAppSession> NewSessionConnected;

        event SessionHandler<TAppSession, CloseReason> SessionClosed;
    }

    public interface IAppServer<TAppSession, TRequestInfo>
        : IAppServer<TAppSession>
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
    {
        event RequestHandler<TAppSession, TRequestInfo> NewRequestReceived;
    }
}
