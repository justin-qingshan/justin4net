using just4net.socket.basic;
using just4net.socket.common;
using just4net.socket.engine;
using just4net.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace just4net.socket.server
{
    public class AsyncSocketServer : TcpSocketServerBase
    {
        public AsyncSocketServer(IAppServer appServer, ListenerInfo[] listeners) 
            : base(appServer, listeners)
        { }

        private BufferManager bufferManager;
        private ConcurrentStack<SocketAsyncEventArgsProxy> readWritePool;

        public override bool Start()
        {
            try
            {
                var config = AppServer.Config;
                int bufferSize = config.RecBufferSize;
                if (bufferSize <= 0)
                    bufferSize = 1024 * 4;

                bufferManager = new BufferManager(bufferSize * config.MaxConnectionNumber, bufferSize);

                try
                {
                    bufferManager.InitBuffer();
                }
                catch(Exception ex)
                {
                    AppServer.Logger.Error("Failed to allocate buffer for async socket event args.", ex);
                    return false;
                }

                SocketAsyncEventArgs args;

                var proxyList = new List<SocketAsyncEventArgsProxy>(config.MaxConnectionNumber);

                for (int i = 0; i < config.MaxConnectionNumber; i++)
                {
                    args = new SocketAsyncEventArgs();
                    bufferManager.SetBuffer(args);
                    proxyList.Add(new SocketAsyncEventArgsProxy(args));
                }

                readWritePool = new ConcurrentStack<SocketAsyncEventArgsProxy>(proxyList);

                if (!base.Start())
                    return false;

                IsRunning = true;
                return true;
            }
            catch (Exception e)
            {
                AppServer.Logger?.Error(e);
                return false;
            }
        }

        protected override void OnNewClientAccepted(ISocketListener listener, Socket client, object state)
        {
            if (IsStopped)
                return;

            ProcessNewClient(client);
        }

        private IAppSession ProcessNewClient(Socket client)
        {
            SocketAsyncEventArgsProxy argsProxy;

            if (!readWritePool.TryPop(out argsProxy))
            {
                AppServer.AsyncRun(client.SafeClose);
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.ErrorFormat("Mac connection number {0} was reached!", AppServer.Config.MaxConnectionNumber);
                return null;
            }

            ISocketSession session = new AsyncSocketSession(client, argsProxy);
            var appSession = CreateSession(client, session);
           
            if (appSession == null)
            {
                argsProxy.Reset();
                readWritePool.Push(argsProxy);
                AppServer.AsyncRun(client.SafeClose);
                return null;
            }

            session.Closed += SessionClosed;
            if (RegisterSession(appSession))
            {
                AppServer.AsyncRun(() => session.Start());
            }
            return appSession;
        }

        private bool RegisterSession(IAppSession session)
        {
            if (AppServer.RegisterSession(session))
                return true;

            session.SocketSession.Close(CloseReason.InternalError);

            return false;
        }

        void SessionClosed(ISocketSession session, CloseReason reason)
        {
            var socketSession = session as IAsyncSocketSessionBase;
            if (socketSession == null)
                return;

            var proxy = socketSession.SocketAsyncProxy;
            proxy.Reset();
            var args = proxy.SocketEventArgs;

            var pool = readWritePool;
            if (pool == null)
            {
                if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                    args.Dispose();
                return;
            }

            if (proxy.OriginOffset != args.Offset)
                args.SetBuffer(proxy.OriginOffset, AppServer.Config.RecBufferSize);

            if (!proxy.IsRecyclable)
            {
                args.Dispose();
                return;
            }

            pool.Push(proxy);
        }

        public override void Stop()
        {
            if (IsStopped)
                return;

            lock (SyncRoot)
            {
                if (IsStopped)
                    return;

                base.Stop();
                foreach (var item in readWritePool)
                    item.SocketEventArgs.Dispose();

                readWritePool = null;
                bufferManager = null;
                IsRunning = false;
            }
        }

    }
}
