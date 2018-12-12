using just4net.socket.common;

namespace just4net.socket.basic
{
    public interface ISocketServer
    {
        bool IsRunning { get; }

        IPoolInfo SendingQueuePool { get; }

        bool Start();

        void Stop();
    }
}
