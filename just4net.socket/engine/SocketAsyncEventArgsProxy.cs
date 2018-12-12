using just4net.socket.basic;
using System;
using System.Net.Sockets;

namespace just4net.socket.engine
{
    class SocketAsyncEventArgsProxy
    {
        public SocketAsyncEventArgs SocketEventArgs { get; private set; }

        public int OriginOffset { get; private set; }

        public bool IsRecyclable { get; private set; }

        private SocketAsyncEventArgsProxy() { }

        public SocketAsyncEventArgsProxy(SocketAsyncEventArgs args) : this(args, true)
        {

        }

        public SocketAsyncEventArgsProxy(SocketAsyncEventArgs args, bool isRecyclable)
        {
            SocketEventArgs = args;
            OriginOffset = args.Offset;
            SocketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEventArgs_Completed);
            IsRecyclable = isRecyclable;
        }

        static void SocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            var socketSession = e.UserToken as IAsyncSocketSession;
            if (socketSession == null)
                return;

            if (e.LastOperation == SocketAsyncOperation.Receive)
                socketSession.AsyncRun(() => socketSession.ProcessReceive(e));
            else
                throw new ArgumentException("The last operation completed on the socket was not a receive.");
        }

        public void Init(IAsyncSocketSession session)
        {
            SocketEventArgs.UserToken = session;
        }

        public void Reset()
        {
            SocketEventArgs.UserToken = null;
        }
    }
}
