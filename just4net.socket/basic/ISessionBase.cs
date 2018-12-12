using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace just4net.socket.basic
{
    public interface ISessionBase
    {
        int SessionID { get; }

        IPEndPoint RemoteEndPoint { get; }
    }
}
