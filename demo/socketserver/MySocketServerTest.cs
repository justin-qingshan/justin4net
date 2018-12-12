using just4net.socket.basic;
using just4net.socket.engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace demo.socketserver
{
    public class MySocketServerTest
    {
        private MyAppServer server;

        public void Start()
        {
            CreateServer();
        }


        private void CreateServer()
        {
            server = new MyAppServer(new MyReceiveFilterFactory());
            ServerConfig serverConfig = new ServerConfig
            {
                Charset = Encoding.UTF8,
                ClearIdleSessionInterval = 10,
                IdleSessionTimeout = 20,
                KeepAliveInterval = 30,
                KeepAliveTime = 30,
                MaxConnectionNumber = 100,
                MaxRequestLength = 8228,
                RecBufferSize = 8228,
                SendBufferSize = 8228,
                SendingQueueSize = 100,
                SendTimeout = 1000,
                Listeners = new List<ListenerConfig>
                {
                    new ListenerConfig { Backlog = 5, Ip = "Any", Port = 13399 }
                }
            };

            bool setupResult = server.Setup(serverConfig, new SocketServerFactory(), new MyReceiveFilterFactory());

            Console.WriteLine($"setup server: {setupResult}");
            if (!setupResult)
                return;

            Console.WriteLine($"start server: {server.Start()}");
        }
    }

    public class Logger : ILog
    {
        public bool IsDebugEnabled { get { return true; } }

        public bool IsErrorEnabled { get { return true; } }

        public bool IsFatalEnabled { get { return true; } }

        public bool IsInfoEnabled { get { return true; } }

        public bool IsWarnEnabled { get { return true; } }

        public void Debug(object message)
        {
            Console.WriteLine("[DEBUG]" + message);
            Console.WriteLine();
        }

        public void Debug(object message, Exception exception)
        {
            Console.WriteLine("[DEBUG]" + message);
            Console.WriteLine(exception);
            Console.WriteLine();
        }

        public void DebugFormat(string format, params object[] args)
        {
            Console.WriteLine("[DEBUG]" + format, args);
            Console.WriteLine();
        }

        public void Error(object message)
        {
            Console.WriteLine("[ERROR]" + message);
            Console.WriteLine();
        }

        public void Error(object message, Exception exception)
        {
            Console.WriteLine("[ERROR]" + message);
            Console.WriteLine(exception);
            Console.WriteLine();
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Console.WriteLine("[ERROR]" + format, args);
            Console.WriteLine();
        }

        public void Fatal(object message)
        {
            Console.WriteLine("[FATAL]" + message);
            Console.WriteLine();
        }

        public void Fatal(object message, Exception exception)
        {
            Console.WriteLine("[FATAL]" + message);
            Console.WriteLine(exception);
            Console.WriteLine();
        }

        public void FatalFormat(string format, params object[] args)
        {
            Console.WriteLine("[FATAL]" + format, args);
            Console.WriteLine();
        }

        public void Info(object message)
        {
            Console.WriteLine("[INFO]" + message);
            Console.WriteLine();
        }

        public void Info(object message, Exception exception)
        {
            Console.WriteLine("[INFO]" + message);
            Console.WriteLine(exception);
            Console.WriteLine();
        }

        public void InfoFormat(string format, params object[] args)
        {
            Console.WriteLine("[INFO]" + format, args);
            Console.WriteLine();
        }

        public void Warn(object message)
        {
            Console.WriteLine("[WARN]" + message);
            Console.WriteLine();
        }

        public void Warn(object message, Exception exception)
        {
            Console.WriteLine("[WARN]" + message);
            Console.WriteLine(exception);
            Console.WriteLine();
        }

        public void WarnFormat(string format, params object[] args)
        {
            Console.WriteLine("[WARN]" + format, args);
            Console.WriteLine();
        }
    }
}
