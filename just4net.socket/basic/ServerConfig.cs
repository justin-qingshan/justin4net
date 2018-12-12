using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace just4net.socket.basic
{
    public class ServerConfig : IServerConfig
    {
        public Encoding Charset { get; set; }

        public int ClearIdleSessionInterval { get; set; }

        public int IdleSessionTimeout { get; set; }

        public int KeepAliveInterval { get; set; }

        public int KeepAliveTime { get; set; }

        public IEnumerable<IListenerConfig> Listeners { get; set; }

        public int MaxConnectionNumber { get; set; }

        public int MaxRequestLength { get; set; }

        public int RecBufferSize { get; set; }

        public int SendBufferSize { get; set; }

        public int SendingQueueSize { get; set; }

        public int SendTimeout { get; set; }
    }
}
