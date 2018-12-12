namespace just4net.socket.basic
{
    public interface IListenerConfig
    {
        string Ip { get; }

        int Port { get; }

        int Backlog { get; }
    }
}
