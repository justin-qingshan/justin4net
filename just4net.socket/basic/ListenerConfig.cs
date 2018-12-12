using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace just4net.socket.basic
{
    public class ListenerConfig : IListenerConfig
    {
        public int Backlog { get; set; }

        public string Ip { get; set; }

        public int Port { get; set; }
    }
}
