using just4net.socket.basic;
using System;
using System.Net.Sockets;

namespace just4net.socket.server
{
    class TcpAsyncSocketListener : SocketListenerBase
    {
        private Socket listenSocket;
        private SocketAsyncEventArgs acceptSAE;

        public TcpAsyncSocketListener(ListenerInfo info) : base(info) { }

        public override bool Start(IServerConfig config)
        {
            listenSocket = new Socket(Info.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenSocket.Bind(Info.EndPoint);
                listenSocket.Listen(Info.BackLog);

                listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                acceptSAE = new SocketAsyncEventArgs();
                acceptSAE.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);

                if (!listenSocket.AcceptAsync(acceptSAE))
                    ProcessAccept(acceptSAE);

                return true;
            }
            catch(Exception e)
            {
                OnError(e);
                return false;
            }
        }

        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket socket = null;

            if (e.SocketError != SocketError.Success)
            {
                int error = (int)e.SocketError;
                if (error == 995 || error == 10004 || error == 10038)
                    return;

                OnError(new SocketException(error));
            }
            else
            {
                socket = e.AcceptSocket;
            }

            e.AcceptSocket = null;
            bool willRaiseEvent = false;
            try
            {
                willRaiseEvent = listenSocket.AcceptAsync(e);
            }
            catch (ObjectDisposedException)
            {
                // The listener was stopped.
                // Do nothing, make sure the ProcessAccept won't be called in the thread.
                willRaiseEvent = true;
            }
            catch (NullReferenceException)
            {
                // The listener was stopped.
                // Do nothing, make sure the ProcessAccept won't be called in the thread.
                willRaiseEvent = true;
            }
            catch (Exception ex)
            {
                OnError(ex);
                willRaiseEvent = true;
            }

            if (socket != null)
                OnNewClientAccepted(socket, null);

            if (!willRaiseEvent)
                ProcessAccept(e);
        }

        public override void Stop()
        {
            if (listenSocket == null)
                return;

            lock (this)
            {
                if (listenSocket == null)
                    return;
                acceptSAE.Completed -= new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
                acceptSAE.Dispose();
                acceptSAE = null;

                try
                {
                    listenSocket.Close();
                }
                finally { listenSocket = null; };
            }

            OnStopped();
        }



    }
}
