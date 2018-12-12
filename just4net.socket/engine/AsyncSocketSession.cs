using just4net.socket.basic;
using just4net.socket.common;
using just4net.socket.protocol;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace just4net.socket.engine
{
    class AsyncSocketSession 
        : SocketSessionBase, IAsyncSocketSession
    {
        private bool isReset;

        // event args used to send data.
        private SocketAsyncEventArgs socketEventArgsSend;

        private IServerConfig config;

        // async proxy which stores a Socket Async Event Args, will be used to receive data.
        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; private set; }

        ILog ILoggerProvider.Logger { get { return logger; } }

        public override int OriginRecOffset { get { return SocketAsyncProxy.OriginOffset; } }

        public override IServerConfig Config { get { return config; } }

        public AsyncSocketSession(Socket client, SocketAsyncEventArgsProxy asyncProxy)
            : this(client, asyncProxy, false)
        {

        }

        public AsyncSocketSession(Socket client, SocketAsyncEventArgsProxy asyncProxy, bool isReset)
            : base(client)
        {
            SocketAsyncProxy = asyncProxy;
            this.isReset = isReset;
        }

        public override void Init(IAppSession appSession)
        {
            base.Init(appSession);
            config = appSession.Config;
            logger = appSession.Logger;
            SocketAsyncProxy.Init(this);

            if (!SyncSend)
            {
                socketEventArgsSend = new SocketAsyncEventArgs();
                socketEventArgsSend.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendingCompleted);
            }
        }

        public override void Start()
        {
            StartReceive(SocketAsyncProxy.SocketEventArgs);

            //if (!isReset)
            //    StartSession();
        } 

        bool ProcessCompleted(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                    return true;
            }
            else
            {
                LogError((int)e.SocketError);
            }

            return false;
        }

        void OnSendingCompleted(object sender, SocketAsyncEventArgs e)
        {
            var queue = e.UserToken as SendingQueue;
            if (!ProcessCompleted(e))
            {
                ClearPrevSendState(e);
                OnSendError(queue, CloseReason.SocketError);
                return;
            }

            var count = queue.Sum(q => q.Count);

            if (count != e.BytesTransferred)
            {
                queue.InternalTrim(e.BytesTransferred);
                logger?.InfoFormat("{0} of {1} bytes was transferred, send the rest {2} bytes now.", e.BytesTransferred, count, queue.Sum(q => q.Count));
                ClearPrevSendState(e);
                SendAsync(queue);
                return;
            }

            ClearPrevSendState(e);
            base.OnSendingCompleted(queue);
        }

        private void ClearPrevSendState(SocketAsyncEventArgs e)
        {
            e.UserToken = null;
            if (e.Buffer != null)
                e.SetBuffer(null, 0, 0);
            else if (e.BufferList != null)
                e.BufferList = null;
        }

        protected override void SendSync(SendingQueue queue)
        {
            try
            {
                for (int i = 0; i < queue.Count; i++)
                {
                    var item = queue[i];
                    var client = Client;
                    if (client == null)
                        return;
                    client.Send(item.Array, item.Offset, item.Count, SocketFlags.None);
                }

                OnSendingCompleted(queue);
            }
            catch(Exception e)
            {
                LogError(e);
                OnSendError(queue, CloseReason.SocketError);
                return;
            }
        }

        protected override void SendAsync(SendingQueue queue)
        {
            try
            {
                socketEventArgsSend.UserToken = queue;

                if (queue.Count > 1)
                    socketEventArgsSend.BufferList = queue;
                else
                    socketEventArgsSend.SetBuffer(queue[0].Array, queue[0].Offset, queue[0].Count);

                var client = Client;
                if (client == null)
                {
                    OnSendError(queue, CloseReason.SocketError);
                    return;
                }

                if (!client.SendAsync(socketEventArgsSend))
                    OnSendingCompleted(client, socketEventArgsSend);
            }
            catch(Exception e)
            {
                LogError(e);
                ClearPrevSendState(socketEventArgsSend);
                OnSendError(queue, CloseReason.SocketError);
            }
        }


        private void StartReceive(SocketAsyncEventArgs e)
        {
            StartReceive(e, 0);
        }

        private void StartReceive(SocketAsyncEventArgs e, int offsetDelta)
        {
            bool willRaiseEvent = false;

            try
            {
                if (offsetDelta < 0 || offsetDelta > config.RecBufferSize)
                    throw new ArgumentException("Illegal offset delta: " + offsetDelta, nameof(offsetDelta));

                int predictOffset = SocketAsyncProxy.OriginOffset + offsetDelta;
                if (e.Offset != predictOffset)
                {
                    e.SetBuffer(predictOffset, config.RecBufferSize - offsetDelta);
                }

                if (!OnReceiveStarted())
                    return;

                willRaiseEvent = Client.ReceiveAsync(e);
            }
            catch (Exception ex)
            {
                LogError(ex);
                OnReceiveTerminated(CloseReason.SocketError);
                return;
            }

            if (!willRaiseEvent)
                ProcessReceive(e);
        }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (!ProcessCompleted(e))
            {
                OnReceiveTerminated(e.SocketError == SocketError.Success ? CloseReason.ClientClosing : CloseReason.SocketError);
                return;
            }

            OnReceiveEnded();
            int offsetDelta;
            try
            {
                offsetDelta = AppSession.ProcessRequest(e.Buffer, e.Offset, e.BytesTransferred, true);
            }
            catch(Exception ex)
            {
                LogError("Protocol error", ex);
                Close(CloseReason.ProtocolError);
                return;
            }

            StartReceive(e, offsetDelta);
        }

        protected override void OnClosed(CloseReason reason)
        {
            var sae = socketEventArgsSend;
            if (sae == null)
            {
                base.OnClosed(reason);
                return;
            }

            if (Interlocked.CompareExchange(ref socketEventArgsSend, null, sae) == sae)
            {
                sae.Dispose();
                base.OnClosed(reason);
            }
        }

        

    }
}
