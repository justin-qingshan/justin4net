using just4net.socket.basic;
using just4net.socket.protocol;
using System;

namespace demo.socketserver
{
    public class MyAppServer : AppServerBase<MySession, MyRequest>
    {
        public MyAppServer(IReceiveFilterFactory<MyRequest> receiveFilterFactory) : base(receiveFilterFactory)
        {
            Logger = new Logger();
            ReceiveFilterFactory = receiveFilterFactory;
        }

        protected override bool Setup()
        {
            NewRequestReceived += (s, r) =>
            {
                Console.WriteLine($"received {s.SessionID} \n[{r.Header.SubType}]{r.Body.ContentStr}");
            };
            return true;
        }

        protected override void OnNewSessionConnected(MySession session)
        {
            base.OnNewSessionConnected(session);
            Console.WriteLine($"Total Session Count: {SessionCount}");
            Console.WriteLine();
        }

        protected override void OnSessionClosed(MySession session, CloseReason reason)
        {
            Console.WriteLine($"Session Closed: {session.SessionID}, caused by: {reason.ToString()}");
            Console.WriteLine($"Total Session Count: {SessionCount}");
            Console.WriteLine();
        }
    }
}
