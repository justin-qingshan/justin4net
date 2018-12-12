namespace just4net.socket.basic
{
    public enum CloseReason
    {
        Unknown = 0,
        ServerShutdown = 1,
        ClientClosing = 2,
        ServerClosing = 3,
        ApplicationError = 4,
        SocketError = 5,
        Timeout = 6,
        ProtocolError = 7,
        InternalError = 8
    }
}
