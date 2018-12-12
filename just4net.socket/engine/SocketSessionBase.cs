using just4net.socket.basic;
using just4net.socket.common;
using just4net.socket.protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace just4net.socket.engine
{
    public abstract partial class SocketSessionBase : ISocketSession
    {
        private static int SessionIdBase = 0;

        protected readonly object sync = new object();

        // 0x00000000
        // 1st byte: Closed(Y/N)
        // 2nd byte: N/A
        // 3nd byte: CloseReason
        // Last byte: normal/sending/receiving/closing
        private int state = 0;

        private Socket client;

        private SendingQueue sendingQueue;

        private ISmartPool<SendingQueue> sendingQueuePool;

        protected ILog logger;

        public IAppSession AppSession { get; private set; }

        protected bool SyncSend { get; private set; }

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime { get; private set; }

        public int SessionID { get; set; }

        public IPEndPoint LocalEndPoint { get; protected set; }

        public IPEndPoint RemoteEndPoint { get; private set; }

        public Action<ISocketSession, CloseReason> Closed { get; set; }

        public abstract int OriginRecOffset { get; }

        public abstract IServerConfig Config { get; }

        public Socket Client { get { return client; } }

        protected bool IsInClosingOrClosed { get { return state >= SocketState.InClosing; } }

        protected bool IsClosed { get { return state >= SocketState.Closed; } }

        #region add or remove state mark.

        private void AddStateFlag(int stateValue)
        {
            AddStateFlag(stateValue, false);
        }

        private bool AddStateFlag(int stateValue, bool notClosing)
        {
            while (true)
            {
                int oldState = state;
                if (notClosing)
                {
                    if (oldState >= SocketState.InClosing)
                        return false;
                }

                var newState = state | stateValue;

                if (Interlocked.CompareExchange(ref state, newState, oldState) == oldState)
                    return true;
            }
        }

        private bool TryAddStateFlag(int stateValue)
        {
            while (true)
            {
                int oldState = state;
                int newState = state | stateValue;
                
                // Already marked by the state value.
                if (oldState == newState)
                    return false;

                if (Interlocked.CompareExchange(ref state, newState, oldState) == oldState)
                    return true;
            }
        }

        private void RemoveStateFlag(int stateValue)
        {
            while (true)
            {
                var oldState = state;
                var newState = state & (~stateValue);

                if (Interlocked.CompareExchange(ref state, newState, oldState) == oldState)
                    return;
            }
        }

        private bool CheckState(int stateValue)
        {
            return (state & stateValue) == stateValue;
        }

        #endregion

        public SocketSessionBase(Socket client)
        {
            SessionID = Interlocked.Increment(ref SessionIdBase);
            this.client = client;
            LocalEndPoint = (IPEndPoint)client.LocalEndPoint;
            RemoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
        }

        public virtual void Init(IAppSession appSession)
        {
            AppSession = appSession;

            if (sendingQueuePool == null)
                sendingQueuePool = ((SocketServerBase)(appSession.AppServer).SocketServer).SendingQueuePool;

            SendingQueue queue;
            if (sendingQueuePool.TryGet(out queue))
            {
                sendingQueue = queue;
                queue.StartEnqueue();
            }
        }

        public abstract void Start();

        #region Sending

        private void InternalSend(ArraySegment<byte> segment)
        {
            if (InternalTrySend(segment))
                return;

            if (Config.SendTimeout < 0)
                throw new TimeoutException("Time out when try to send data.");

            var timeOutTime = Config.SendTimeout > 0 ? DateTime.Now.AddMilliseconds(Config.SendTimeout) : DateTime.Now;
            var spinWait = new SpinWait();

            while (!IsInClosingOrClosed)
            {
                spinWait.SpinOnce();

                if (InternalTrySend(segment))
                    return;

                if (Config.SendTimeout > 0 && DateTime.Now >= timeOutTime)
                    throw new TimeoutException("Time out when try to send data.");
            }
        }

        private bool InternalTrySend(ArraySegment<byte> segment)
        {
            if (!TrySend(segment))
                return false;

            LastActiveTime = DateTime.Now;
            return true;
        }

        public bool TrySend(IList<ArraySegment<byte>> segments)
        {
            if (IsClosed)
                return false;

            var queue = sendingQueue;
            if (queue == null)
                return false;

            var trackId = queue.TrackID;
            if (!queue.Enqueue(segments, trackId))
                return false;

            StartSend(queue, trackId, true);
            return true;
        }

        public bool TrySend(ArraySegment<byte> segment)
        {
            if (IsClosed)
                return false;

            var queue = sendingQueue;
            if (queue == null)
                return false;

            var trackId = queue.TrackID;
            if (queue.Enqueue(segment, trackId))
                return false;

            StartSend(queue, trackId, true);
            return true;
        }

        protected abstract void SendAsync(SendingQueue queue);

        protected abstract void SendSync(SendingQueue queue);

        private void Send(SendingQueue queue)
        {
            if (SyncSend)
                SendSync(queue);
            else
                SendAsync(queue);
        }

        private void StartSend(SendingQueue queue, int sendingTrackId, bool initial)
        {
            if (initial)
            {
                if (!TryAddStateFlag(SocketState.InSending))
                    return;

                var currentQueue = sendingQueue;
                if (currentQueue != queue || sendingTrackId != currentQueue.TrackID)
                {
                    OnSendEnd();
                    return;
                }
            }

            Socket socket;

            if (IsInClosingOrClosed && TryValidateClosedBySocket(out socket))
            {
                OnSendEnd();
                return;
            }

            SendingQueue newQueue;

            if (!sendingQueuePool.TryGet(out newQueue))
            {
                OnSendEnd(CloseReason.InternalError, true);
                logger?.Error("There is no enough available sending queue!");
                return;
            }

            var oldQueue = Interlocked.CompareExchange(ref sendingQueue, newQueue, queue);
            if (!ReferenceEquals(oldQueue, queue))
            {
                if (newQueue != null)
                    sendingQueuePool.Push(newQueue);

                if (IsInClosingOrClosed)
                {
                    OnSendEnd();
                }
                else
                {
                    OnSendEnd(CloseReason.InternalError, true);
                    logger?.Error("Failed to switch the sending queue.");
                }
                return;
            }

            newQueue.StartEnqueue();
            queue.StopEnqueue();

            if (queue.Count == 0)
            {
                sendingQueuePool.Push(queue);
                OnSendEnd(CloseReason.InternalError, true);
                logger?.Error("There is no available data to be sent in the queue!");
                return;
            }

            Send(queue);
        }

        protected virtual void OnSendingCompleted(SendingQueue queue)
        {
            queue.Clear();
            sendingQueuePool.Push(queue);
            var newQueue = sendingQueue;

            if (IsInClosingOrClosed)
            {
                Socket socket;
                if (newQueue.Count > 0 && !TryValidateClosedBySocket(out socket))
                {
                    StartSend(newQueue, newQueue.TrackID, false);
                    return;
                }

                OnSendEnd();
                return;
            }

            if (newQueue.Count == 0)
            {
                OnSendEnd();
                if (newQueue.Count > 0)
                    StartSend(newQueue, newQueue.TrackID, true);
            }
            else
                StartSend(newQueue, newQueue.TrackID, false);
        }

        private void OnSendEnd()
        {
            OnSendEnd(CloseReason.Unknown, false);
        }

        private void OnSendEnd(CloseReason reason, bool forceClose)
        {
            RemoveStateFlag(SocketState.InSending);
            ValidateClosed(reason, forceClose, true);
        }

        protected void OnSendError(SendingQueue queue, CloseReason reason)
        {
            queue.Clear();
            sendingQueuePool.Push(queue);
            OnSendEnd(reason, true);
        }

        #endregion

        protected bool OnReceiveStarted()
        {
            if (AddStateFlag(SocketState.InReceiving, true))
                return true;

            ValidateClosed(CloseReason.Unknown, false);
            return false;
        }

        protected void OnReceiveTerminated(CloseReason reason)
        {
            OnReceiveEnded();
            ValidateClosed(reason, true);
        }

        protected void OnReceiveEnded()
        {
            RemoveStateFlag(SocketState.InReceiving);
        }

        protected virtual void OnClosed(CloseReason reason)
        {
            if (!TryAddStateFlag(SocketState.Closed))
                return;

            while (true)
            {
                var queue = sendingQueue;
                if (queue == null)
                    break;

                if (Interlocked.CompareExchange(ref sendingQueue, null, queue) == queue)
                {
                    queue.Clear();
                    sendingQueuePool.Push(queue);
                    break;
                }
            }

            Closed?.Invoke(this, reason);
        }

        private bool ValidateNotInSendingReceiving()
        {
            int oldState = state;
            return (oldState & SocketState.InSendingRecivingMask) == oldState;
        }

        protected virtual bool TryValidateClosedBySocket(out Socket socket)
        {
            socket = client;
            return socket == null;
        }

        public virtual void Close(CloseReason reason)
        {
            if (!TryAddStateFlag(SocketState.InClosing))
                return;

            Socket socket;

            if (TryValidateClosedBySocket(out socket))
                return;

            if (CheckState(SocketState.InSending))
            {
                AddStateFlag(GetCloseReasonValue(reason));
            }

            if (socket != null)
                InternalClose(socket, reason, true);
            else
                OnClosed(reason);
        }

        private void InternalClose(Socket client, CloseReason reason, bool setCloseReason)
        {
            if (Interlocked.CompareExchange(ref this.client, null, client) == client)
            {
                if (setCloseReason)
                    AddStateFlag(GetCloseReasonValue(reason));

                client.SafeClose();

                if (ValidateNotInSendingReceiving())
                    OnClosed(reason);
            }
        }

        private const int CloseReasonMagic = 256;

        private int GetCloseReasonValue(CloseReason reason)
        {
            return ((int)reason + 1) * CloseReasonMagic;
        }

        private CloseReason GetCloseReasonFromState()
        {
            return (CloseReason)(state / CloseReasonMagic - 1);
        }

        private void FireCloseEvent()
        {
            OnClosed(GetCloseReasonFromState());
        }

        private void ValidateClosed()
        {
            ValidateClosed(CloseReason.Unknown, false);
        }

        private void ValidateClosed(CloseReason reason, bool forceClose)
        {
            ValidateClosed(reason, forceClose, false);
        }

        private void ValidateClosed(CloseReason reason, bool forceClose, bool forSend)
        {
            lock (this)
            {
                if (IsClosed)
                    return;

                if (CheckState(SocketState.InClosing))
                {
                    if (forSend)
                    {
                        Socket client;
                        if (!TryValidateClosedBySocket(out client))
                        {
                            var queue = sendingQueue;
                            if (forceClose || (sendingQueue != null && sendingQueue.Count == 0))
                            {
                                if (client != null)
                                    InternalClose(client, GetCloseReasonFromState(), false);
                                else
                                    FireCloseEvent();
                            }
                            return;
                        }
                    }

                    if (ValidateNotInSendingReceiving())
                        FireCloseEvent();
                }
                else if (forceClose)
                {
                    Close(reason);
                }
            }
        }

        protected virtual bool IsIgnorableSocketError(int socketError)
        {
            if (socketError == 10004
                || socketError == 10053
                || socketError == 10054
                || socketError == 10058
                || socketError == 10060
                || socketError == 994
                || socketError == -1073741299)
                return true;

            return false;
        }

        protected virtual bool IsIgnorableException(Exception e, out int socketErrorCode)
        {
            socketErrorCode = 0;

            if (e is ObjectDisposedException || e is NullReferenceException)
                return true;

            SocketException socketException = null;

            if (e is IOException)
            {
                if (e.InnerException is ObjectDisposedException || e.InnerException is NullReferenceException)
                    return true;

                socketException = e.InnerException as SocketException;
            }
            else
                socketException = e as SocketException;

            if (socketException == null)
                return false;

            socketErrorCode = socketException.ErrorCode;

            return IsIgnorableSocketError(socketErrorCode);
        }
    }
}
