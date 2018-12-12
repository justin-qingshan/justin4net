using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using just4net.socket.protocol;
using just4net.socket.basic;
using System.Net;
using just4net.util;
using System.Threading;
using System.Collections.Concurrent;

namespace just4net.socket.basic
{
    public abstract class AppServerBase<TAppSession, TRequestInfo>
        : IAppServer<TAppSession, TRequestInfo>, IRequestHandler<TRequestInfo>, IDisposable
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSessionBase<TAppSession, TRequestInfo>, IAppSession, new()
    {
        protected readonly TAppSession Null = default(TAppSession);

        private int state = ServerStateConst.NotInitialized;
        private ISocketServerFactory socketServerFactory;
        private ISocketServer socketServer;
        private static bool threadPoolConfigured = false;
        private ListenerInfo[] listeners;

        private ConcurrentDictionary<int, TAppSession> sessionDict = new ConcurrentDictionary<int, TAppSession>();

        
        public ServerState State { get { return (ServerState)state; } }
        
        public ListenerInfo[] Listeners { get { return listeners; } }

        public IServerConfig Config { get; private set; }

        public virtual IReceiveFilterFactory<TRequestInfo> ReceiveFilterFactory { get; protected set; }
        

        public virtual List<IConnectionFilter> ConnectionFilters { get; protected set; }

        ISocketServer IAppServer.SocketServer { get { return socketServer; } }

        public DateTime StartTime { get; private set; }

        public Encoding Encoding { get; private set; }

        public ILog Logger { get; protected set; }


        public AppServerBase() { }

        public AppServerBase(IReceiveFilterFactory<TRequestInfo> receiveFilterFactory)
        {
            ReceiveFilterFactory = receiveFilterFactory;
        }

        public bool Setup(IServerConfig config, ISocketServerFactory socketServerFactory = null,
            IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null,
            IEnumerable<IConnectionFilter> connectionFilters = null)
        {
            TrySetInitializedState();

            SetupBasic(config, socketServerFactory);

            Setup2(receiveFilterFactory, connectionFilters);

            if (!Setup())
                return false;

            if (!Setup3(config))
                return false;

            state = ServerStateConst.NotStarted;
            return true;
        }

        private void TrySetInitializedState()
        {
            if (Interlocked.CompareExchange(ref state, ServerStateConst.Initializing, ServerStateConst.NotInitialized)
                != ServerStateConst.NotInitialized)
                throw new Exception("Ther server has been initilized!");
        }

        private void SetupBasic(IServerConfig config, ISocketServerFactory socketServerFactory)
        {
            Config = config;
            Encoding = config.Charset;
            this.socketServerFactory = socketServerFactory;
        }

        private void Setup2(IReceiveFilterFactory<TRequestInfo> receiveFilterFactory,
            IEnumerable<IConnectionFilter> connectionFilters)
        {
            if (receiveFilterFactory != null)
                ReceiveFilterFactory = receiveFilterFactory;

            if (connectionFilters != null && connectionFilters.Any())
            {
                if (ConnectionFilters == null)
                    ConnectionFilters = new List<IConnectionFilter>();
                ConnectionFilters.AddRange(connectionFilters);
            }
        }

        private bool Setup3(IServerConfig config)
        {
            if (!SetupListeners(config))
                return false;

            return SetupSocketServer();
        }

        private bool SetupListeners(IServerConfig config)
        {
            var listeners = new List<ListenerInfo>();
            try
            {
                if (config.Listeners != null && config.Listeners.Any())
                {
                    foreach(IListenerConfig lConfig in config.Listeners)
                    {
                        listeners.Add(new ListenerInfo
                        {
                            EndPoint = new IPEndPoint(ParseIPAddress(lConfig.Ip), lConfig.Port),
                            BackLog = lConfig.Backlog
                        });
                    }
                }

                if (!listeners.Any())
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error("No listener found!");
                    return false;
                }
                this.listeners = listeners.ToArray();
                return true;
            }
            catch(Exception ex)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(ex);
                return false;
            }
        }

        private bool SetupSocketServer()
        {
            try
            {
                socketServer = socketServerFactory.CreateSocketServer<TRequestInfo>(this, listeners, Config);
                return socketServer != null;
            }
            catch(Exception ex)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(ex);
                return false;
            }
        }

        protected virtual bool Setup()
        {
            return true;
        }

        private IPAddress ParseIPAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip) || "Any".Eq(ip, true))
                return IPAddress.Any;
            else if ("IPv6Any".Eq(ip, true))
                return IPAddress.IPv6Any;
            else
                return IPAddress.Parse(ip);
        }

        public virtual bool Start()
        {
            int originState = Interlocked.CompareExchange(ref state, ServerStateConst.Starting, ServerStateConst.NotStarted);
            if (originState != ServerStateConst.NotStarted)
            {
                if (originState < ServerStateConst.NotStarted)
                    throw new Exception("The server need to be setup first!");

                if (Logger.IsErrorEnabled)
                    Logger.Error($"Can't start there server in the state {(ServerState)originState}");

                return false;
            }

            if (!socketServer.Start())
            {
                state = ServerStateConst.NotStarted;
                return false;
            }

            StartTime = DateTime.Now;
            state = ServerStateConst.Running;

            try
            {
                OnStarted();
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
            finally { Logger.Info($"There server {GetType()} has been started!"); }

            StartClearSessionTimer();

            return true;
        }

        protected virtual void OnStarted()
        {

        }

        protected virtual void OnStopped()
        {

        }

        public virtual void Stop()
        {
            if (Interlocked.CompareExchange(ref state, ServerStateConst.Stopping, ServerStateConst.Running)
                != ServerStateConst.Running)
                return;

            socketServer.Stop();
            state = ServerStateConst.NotStarted;

            OnStopped();
            Logger.Info($"The server {GetType()} has been stopped!");

            if (clearIdleSessionTimer != null)
            {
                clearIdleSessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                clearIdleSessionTimer.Dispose();
                clearIdleSessionTimer = null;
            }

            var sessions = sessionDict.ToArray();
            if (sessions.Length > 0)
            {
                var tasks = new Task[sessions.Length];
                for(int i = 0; i< tasks.Length; i++)
                {
                    tasks[i] = Task.Factory.StartNew(s =>
                    {
                        var session = s as TAppSession;
                        if (session != null)
                            session.Close(CloseReason.ServerShutdown);
                    }, sessions[i].Value);
                }

                Task.WaitAll(tasks);
            }
        }

        private RequestHandler<TAppSession, TRequestInfo> requestHandler;

        public virtual event RequestHandler<TAppSession, TRequestInfo> NewRequestReceived
        {
            add { requestHandler += value; }
            remove { requestHandler -= value; }
        }

        internal void HandleRequest(IAppSession session, TRequestInfo request)
        {
            HandleRequest((TAppSession)session, request);
        }

        void IRequestHandler<TRequestInfo>.HandleRequest(IAppSession session, TRequestInfo request)
        {
            HandleRequest((TAppSession)session, request);
        }

        protected virtual void HandleRequest(TAppSession session, TRequestInfo request)
        {
            try
            {
                requestHandler(session, request);
            }
            catch(Exception ex)
            {
                session.InternalHandleException(ex);
            }
        }

        private bool ExecuteConnectionFilters(IPEndPoint remoteEndPoint)
        {
            if (ConnectionFilters == null)
                return true;

            foreach(var filter in ConnectionFilters)
            {
                if (!filter.AllowConnect(remoteEndPoint))
                {
                    if (Logger.IsInfoEnabled)
                        Logger.InfoFormat("A connection from {0} has been refused by {1}", remoteEndPoint, filter.GetType().ToString());
                    return false;
                }
            }

            return true;
        }

        IAppSession IAppServer.CreateAppSession(ISocketSession socketSession)
        {
            if (!ExecuteConnectionFilters(socketSession.RemoteEndPoint))
                return Null;

            var appSession = CreateAppSession(socketSession);
            appSession.Init(this, socketSession);
            return appSession;
        }

        protected virtual TAppSession CreateAppSession(ISocketSession socketSession)
        {
            return new TAppSession();
        }

        bool IAppServer.RegisterSession(IAppSession session)
        {
            var appSession = session as TAppSession;
            if (!RegisterSession(appSession.SessionID, appSession))
                return false;

            appSession.SocketSession.Closed += OnSocketSessionClosed;

            if (Logger.IsInfoEnabled)
                Logger.Info(session, "A new session connected!");

            OnNewSessionConnected(appSession);
            return true;
        }

        protected virtual bool RegisterSession(int sessionId, TAppSession appSession)
        {
            if (sessionDict.TryAdd(sessionId, appSession))
                return true;

            if (Logger.IsErrorEnabled)
                Logger.Error(appSession, "The session is refused because of duplicate id.");

            return false;
        }

        private SessionHandler<TAppSession> newSessionConnected;

        public event SessionHandler<TAppSession> NewSessionConnected
        {
            add { newSessionConnected += value; }
            remove { newSessionConnected -= value; }
        }

        protected virtual void OnNewSessionConnected(TAppSession session)
        {
            var handler = newSessionConnected;
            if (handler == null)
                return;

            handler.BeginInvoke(session, OnNewSessionConnectedCallback, handler);
        }

        private void OnNewSessionConnectedCallback(IAsyncResult result)
        {
            try
            {
                var handler = (SessionHandler<TAppSession>)result.AsyncState;
                handler.EndInvoke(result);
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
        }

        private void OnSocketSessionClosed(ISocketSession session, CloseReason reason)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info(session, $"The session was closed for {reason}");

            var appSession = session.AppSession as TAppSession;
            appSession.Connected = false;
            OnSessionClosed(appSession, reason);
        }

        private SessionHandler<TAppSession, CloseReason> sessionClosed;

        public event SessionHandler<TAppSession, CloseReason> SessionClosed
        {
            add { sessionClosed += value; }
            remove { sessionClosed -= value; }
        }

        protected virtual void OnSessionClosed(TAppSession session, CloseReason reason)
        {
            TAppSession removed;
            if (!sessionDict.TryRemove(session.SessionID, out removed))
                if (Logger.IsErrorEnabled)
                    Logger.Error(session, "Failed to remove session.");

            var handler = sessionClosed;
            if (handler != null)
            {
                handler.BeginInvoke(session, reason, OnSessionClosedCallback, handler);
            }
            session.OnSessionClosed(reason);
        }

        private void OnSessionClosedCallback(IAsyncResult result)
        {
            try
            {
                var handler = (SessionHandler<TAppSession, CloseReason>)result.AsyncState;
                handler.EndInvoke(result);
            }
            catch(Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public virtual TAppSession GetSessionByID(int sessionId)
        {
            TAppSession targetSession;
            sessionDict.TryGetValue(sessionId, out targetSession);
            return targetSession;
        }

        IAppSession IAppServer.GetSessionByID(int sessionId)
        {
            return GetSessionByID(sessionId);
        }

        public virtual IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera)
        {
            var sessionSource = SessionSource;
            if (sessionSource == null)
                return null;

            return sessionSource.Select(p => p.Value).Where(critera);
        }

        public virtual IEnumerable<TAppSession> GetAllSession()
        {
            var sessionSource = SessionSource;
            if (sessionSource == null)
                return null;
            return sessionSource.Select(p => p.Value);
        }

        public virtual int SessionCount { get { return sessionDict.Count; } }

        public void Dispose()
        {
            if (state == ServerStateConst.Running)
                Stop();
        }

        private Timer clearIdleSessionTimer = null;

        private void StartClearSessionTimer()
        {
            int interval = Config.ClearIdleSessionInterval * 1000;
            clearIdleSessionTimer = new Timer(ClearIdleSession, new object(), interval, interval);
        }

        private KeyValuePair<int, TAppSession>[] SessionSource { get { return sessionDict.ToArray(); } }

        private void ClearIdleSession(object state)
        {
            if (Monitor.TryEnter(state))
            {
                try
                {
                    var sessionSource = SessionSource;
                    if (sessionSource == null)
                        return;

                    DateTime now = DateTime.Now;
                    DateTime timeout = now.AddSeconds(0 - Config.IdleSessionTimeout);
                    var timeoutSessions = sessionSource.Where(s => s.Value.LastActiveTime <= timeout).Select(s => s.Value);

                    if (Logger.IsInfoEnabled)
                        Logger.Info($"Cleart Idle Session: {timeoutSessions.Count()} / {sessionSource.Count()}");

                    Parallel.ForEach(timeoutSessions, s =>
                    {
                        if (Logger.IsInfoEnabled)
                            Logger.Info(s, $"The session will be closed for timeout, the session start time : {s.StartTime}, last active time: {s.LastActiveTime}");
                        s.Close(CloseReason.Timeout);
                    });
                }
                catch(Exception ex)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error("Clear idle session error!", ex);
                }
                finally
                {
                    Monitor.Exit(state);
                }
            }
        }


    }
}
