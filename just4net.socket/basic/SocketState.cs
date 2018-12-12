namespace just4net.socket.basic
{
    public static class SocketState
    {
        public const int Normal = 0;        // 0000 0000
        public const int InSending = 1;     // 0000 0001
        public const int InReceiving = 2;   // 0000 0010
        public const int InSendingRecivingMask = -4;    // ~(InSending | InReceiving)

        public const int InClosing = 16;    // 0001 0000
        public const int Closed = 1677216;  // 0100 0000
    }
}
