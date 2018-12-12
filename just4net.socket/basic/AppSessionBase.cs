using just4net.socket.protocol;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace just4net.socket.basic
{
    public abstract class AppSessionBase<TAppSession, TRequestInfo>
        : IAppSession, IAppSession<TAppSession, TRequestInfo>
        where TAppSession : AppSessionBase<TAppSession, TRequestInfo>, IAppSession, new()
        where TRequestInfo: class, IRequestInfo
    {
        private bool connected = false;

        IReceiveFilter<TRequestInfo> receiveFilter;

        public virtual AppServerBase<TAppSession, TRequestInfo> AppServer { get; private set; }

        IAppServer IAppSession.AppServer { get { return AppServer; } }

        public Encoding Charset { get; set; }

        public bool Connected
        {
            get { return connected; }
            internal set { connected = value; }
        }

        public IPEndPoint LocalEndPoint { get { return SocketSession.LocalEndPoint; } }

        public IPEndPoint RemoteEndPoint { get { return SocketSession.RemoteEndPoint; } }

        public ILog Logger { get { return AppServer.Logger; } }

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime { get; set; }

        public int SessionID { get; private set; }

        public ISocketSession SocketSession { get; private set; }

        public IServerConfig Config { get { return AppServer.Config; } }

        public AppSessionBase()
        {
            StartTime = DateTime.Now;
            LastActiveTime = StartTime;
        }

        public virtual void Init(IAppServer<TAppSession, TRequestInfo> appServer, ISocketSession socketSession)
        {
            var server = (AppServerBase<TAppSession, TRequestInfo>)appServer;
            AppServer = server;
            Charset = AppServer.Config.Charset;
            SocketSession = socketSession;
            SessionID = socketSession.SessionID;
            connected = true;
            receiveFilter = server.ReceiveFilterFactory.CreateFilter(appServer, this, socketSession.RemoteEndPoint);

            socketSession.Init(this);

            OnInit();
        }

        void IAppSession.StartSession()
        {
            OnSessionStarted();
        }

        protected virtual void OnInit() { }

        protected virtual void OnSessionStarted() { }

        internal protected virtual void OnSessionClosed(CloseReason reason) { }

        protected virtual void HandleException(Exception ex)
        {
            AppServer.Logger.Error(this, ex);
            Close(CloseReason.ApplicationError);
        }

        protected virtual void HandleUnknownRequest(TRequestInfo request) { }

        internal void InternalHandleUnknownRequest(TRequestInfo request)
        {
            HandleUnknownRequest(request);
        }

        internal void InternalHandleException(Exception ex)
        {
            HandleException(ex);
        }

        public virtual void Close(CloseReason reason)
        {
            SocketSession.Close(reason);
        }

        public virtual void Close()
        {
            Close(CloseReason.ServerClosing);
        }

        public virtual bool TrySend(byte[] data, int offset, int length)
        {
            return InternalTrySend(new ArraySegment<byte>(data, offset, length));
        }

        public virtual void Send(byte[] data, int offset, int length)
        {
            InternalSend(new ArraySegment<byte>(data, offset, length));
        }

        private bool InternalTrySend(ArraySegment<byte> segment)
        {
            if (!SocketSession.TrySend(segment))
                return false;

            LastActiveTime = DateTime.Now;
            return true;
        }

        private void InternalSend(ArraySegment<byte> segment)
        {
            if (!connected)
                return;

            if (InternalTrySend(segment))
                return;

            int sendTimeout = Config.SendTimeout;

            if (sendTimeout < 0)
            {
                throw new TimeoutException("The sending timed out!");
            }

            DateTime timeoutTime = sendTimeout > 0 ? DateTime.Now.AddMilliseconds(sendTimeout) : DateTime.Now;
            var spinWait = new SpinWait();
            while (connected)
            {
                spinWait.SpinOnce();
                if (InternalTrySend(segment))
                    return;

                if (sendTimeout > 0 && DateTime.Now >= timeoutTime)
                    throw new TimeoutException("The sending timed out!");
            }
        }

        TRequestInfo FilterRequest(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest, out int offsetDelta)
        {
            int currentRequestLength = receiveFilter.LeftBufferSize;
            TRequestInfo request = receiveFilter.Filter(readBuffer, offset, length, toBeCopied, out rest);
            if (receiveFilter.State == FilterState.Error)
            {
                rest = 0;
                offsetDelta = 0;
                Close(CloseReason.ProtocolError);
                return null;
            }

            offsetDelta = receiveFilter.OffsetDelta;
            if (request == null)
                currentRequestLength = receiveFilter.LeftBufferSize;
            else
                currentRequestLength = currentRequestLength + length - rest;

            if (currentRequestLength >= Config.MaxRequestLength)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(this, $"current processed length: {currentRequestLength}/{Config.MaxRequestLength}");
                Close(CloseReason.ProtocolError);
                return null;
            }

            return request;
        }

        int IAppSession.ProcessRequest(byte[] readBuffer, int offset, int length, bool toBeCopied)
        {
            int rest, offsetDelta;

            while (true)
            {
                var request = FilterRequest(readBuffer, offset, length, toBeCopied, out rest, out offsetDelta);
                if (request != null)
                {
                    try
                    {
                        AppServer.HandleRequest(this, request);
                        LastActiveTime = DateTime.Now;
                    }
                    catch(Exception e) { HandleException(e); }
                }

                if (rest <= 0)
                    return offsetDelta;

                offset = offset + length - rest;
                length = rest;
            }
        }
    }



}
