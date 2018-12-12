using just4net.socket.basic;
using just4net.socket.common;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace just4net.socket.engine
{
    public abstract class SocketServerBase : ISocketServer, IDisposable
    {
        protected object SyncRoot = new object();

        public IAppServer AppServer { get; private set; }

        public bool IsRunning { get; protected set; }

        protected ListenerInfo[] ListenersInfo { get; private set; }

        protected List<ISocketListener> Listeners { get; private set; }

        protected bool IsStopped { get; private set; }

        public ISmartPool<SendingQueue> SendingQueuePool { get; private set; }

        IPoolInfo ISocketServer.SendingQueuePool { get { return SendingQueuePool; } }

        public SocketServerBase(IAppServer appServer, ListenerInfo[] listeners)
        {
            AppServer = appServer;
            IsRunning = false;
            ListenersInfo = listeners;
            Listeners = new List<ISocketListener>(listeners.Length);
        }

        public virtual bool Start()
        {
            IsStopped = false;

            var config = AppServer.Config;
            var logger = AppServer.Logger;
            SendingQueuePool = new SmartPool<SendingQueue>();
            SendingQueuePool.Init(
                Math.Max(config.MaxConnectionNumber / 6, 256),
                Math.Max(config.MaxConnectionNumber * 2, 256),
                new SendingQueueSourceCreator(config.SendingQueueSize));

            for (int i = 0; i < ListenersInfo.Length; i++)
            {
                var listener = CreateListener(ListenersInfo[i]);
                listener.Error += new ErrorHandler(OnListenerError);
                listener.Stopped += new EventHandler(OnListenerStopped);
                listener.NewClientAccepted += new NewClientAcceptHandler(OnNewClientAccepted);

                if (listener.Start(config))
                {
                    Listeners.Add(listener);
                    if (logger.IsDebugEnabled)
                        logger.DebugFormat("Listener ({0}) was started", listener.EndPoint);
                }
                else
                {
                    if (logger.IsDebugEnabled)
                        logger.DebugFormat("Listener ({0}) failed to start", listener.EndPoint);

                    for (int k = 0; k < Listeners.Count; k++)
                        Listeners[k].Stop();

                    Listeners.Clear();
                    return false;
                }
            }

            IsRunning = true;
            return true;
        }

        protected abstract void OnNewClientAccepted(ISocketListener listener, Socket client, object state);

        void OnListenerError(ISocketListener listener, Exception ex)
        {
            if (AppServer.Logger.IsErrorEnabled)
                AppServer.Logger.Error(string.Format("Listener ({0}) error: {1}", listener.EndPoint, ex.Message), ex);
        }

        void OnListenerStopped(object sender, EventArgs e)
        {
            if (AppServer.Logger.IsDebugEnabled)
                AppServer.Logger.DebugFormat("Listener ({0}) was stopped.", (sender as ISocketListener).EndPoint);
        }

        protected abstract ISocketListener CreateListener(ListenerInfo listenerInfo);

        public virtual void Stop()
        {
            IsStopped = true;
            for(int i = 0; i < Listeners.Count; i++)
            {
                Listeners[i].Stop();
            }

            Listeners.Clear();
            SendingQueuePool = null;
            IsRunning = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRunning)
                    Stop();
            }
        }
    }
}
