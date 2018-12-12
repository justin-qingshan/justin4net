using just4net.socket.basic;
using System.Net.Sockets;

namespace just4net.socket.engine
{
    interface IAsyncSocketSessionBase : ILoggerProvider
    {
        SocketAsyncEventArgsProxy SocketAsyncProxy { get; }

        Socket Client { get; }
    }

    interface IAsyncSocketSession : IAsyncSocketSessionBase
    {
        void ProcessReceive(SocketAsyncEventArgs e);
    }
}
