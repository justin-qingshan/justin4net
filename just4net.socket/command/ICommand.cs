using just4net.socket.basic;
using just4net.socket.protocol;

namespace just4net.socket.command
{
    public interface ICommand
    {
        string Name { get; }
    }

    public interface ICommand<TSocketSession, TRequestInfo> : ICommand
        where TSocketSession : ISocketSession
        where TRequestInfo: class, IRequestInfo
    {
        void ExecuteCommand(TSocketSession session, TRequestInfo requestInfo);
    }
}
