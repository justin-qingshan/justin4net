using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using just4net.socket.basic;

namespace demo.socketserver
{
    public class MySession : AppSessionBase<MySession, MyRequest>
    {
        public int appid;

    }
}
