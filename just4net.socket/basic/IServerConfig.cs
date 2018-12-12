using System.Collections.Generic;
using System.Text;

namespace just4net.socket.basic
{
    public interface IServerConfig
    {
        int RecBufferSize { get; } 

        int SendBufferSize { get; }

        Encoding Charset { get; }

        int SendTimeout { get; }

        int MaxRequestLength { get; }

        int MaxConnectionNumber { get; }

        int SendingQueueSize { get; }

        int KeepAliveTime { get; }

        int KeepAliveInterval { get; }

        IEnumerable<IListenerConfig> Listeners { get; }

        int IdleSessionTimeout { get; }

        int ClearIdleSessionInterval { get; }
    }
}
