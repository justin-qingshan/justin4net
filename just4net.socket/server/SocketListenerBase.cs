using just4net.socket.basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace just4net.socket.server
{
    public abstract class SocketListenerBase : ISocketListener
    {
        public ListenerInfo Info { get; private set; }

        public IPEndPoint EndPoint { get { return Info.EndPoint; } }

        public SocketListenerBase(ListenerInfo info)
        {
            Info = info;
        }

        public abstract bool Start(IServerConfig config);

        public abstract void Stop();

        public event NewClientAcceptHandler NewClientAccepted;

        public event ErrorHandler Error;

        public event EventHandler Stopped;

        protected void OnError(Exception e)
        {
            Error?.Invoke(this, e);
        }

        protected virtual void OnNewClientAccepted(Socket socket, object state)
        {
            NewClientAccepted?.Invoke(this, socket, state);
        }

        protected void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }
}
