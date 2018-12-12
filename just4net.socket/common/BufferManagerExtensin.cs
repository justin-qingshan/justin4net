using just4net.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace just4net.socket.common
{
    public static class BufferManagerExtensin
    {
        public static bool SetBuffer(this BufferManager source, SocketAsyncEventArgs args)
        {
            int offset;
            if (source.SetBuffer(out offset))
            {
                args.SetBuffer(source.Buffer, offset, source.BufferSize);
                return true;
            }
            return false;
        }
    }
}
