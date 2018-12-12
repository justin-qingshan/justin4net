namespace just4net.socket.basic
{
    public enum ServerState : int
    {
        NotInitialized = ServerStateConst.NotInitialized,
        Initializing = ServerStateConst.Initializing,
        NotStarted = ServerStateConst.NotStarted,
        Starting = ServerStateConst.Starting,
        Running = ServerStateConst.Running,
        Stopping = ServerStateConst.Stopping
    }

    internal class ServerStateConst
    {
        public const int NotInitialized = 0;
        public const int Initializing = 1;
        public const int NotStarted = 2;
        public const int Starting = 3;
        public const int Running = 4;
        public const int Stopping = 5;
    }
}
