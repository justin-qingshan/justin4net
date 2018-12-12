using System.Net;

namespace just4net.socket.basic
{
    public class ListenerInfo
    {
        public int BackLog { get; set; }

        public IPEndPoint EndPoint { get; set; }
    }
}
