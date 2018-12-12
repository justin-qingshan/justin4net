using System;
using System.Net;
using System.Net.Sockets;

namespace just4net.socket.basic
{
    public delegate void ErrorHandler(ISocketListener listener, Exception ex);
    public delegate void NewClientAcceptHandler(ISocketListener listener, Socket client, object state);
    

    public interface ISocketListener
    {
        IPEndPoint EndPoint { get; }

        bool Start(IServerConfig config);

        void Stop();

        event NewClientAcceptHandler NewClientAccepted;

        event ErrorHandler Error;

        event EventHandler Stopped;
        
    }
}
