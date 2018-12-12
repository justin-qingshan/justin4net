using just4net.socket.basic;
using just4net.socket.engine;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace just4net.socket.server
{
    public abstract class TcpSocketServerBase : SocketServerBase
    {
        private readonly byte[] keeyAliveOptionValues;
        private readonly byte[] keepAliveOptionOutValues;

        private readonly int sendTimeout;
        private readonly int recBufferSize;
        private readonly int sendBufferSize;

        public TcpSocketServerBase(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
        {
            var config = AppServer.Config;
            uint dummy = 0;
            keeyAliveOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            keeyAliveOptionValues = new byte[keeyAliveOptionValues.Length];

            BitConverter.GetBytes((uint)1).CopyTo(keeyAliveOptionValues, 0);
            BitConverter.GetBytes((uint)config.KeepAliveTime * 1000).CopyTo(keeyAliveOptionValues, Marshal.SizeOf(dummy));
            BitConverter.GetBytes((uint)config.KeepAliveInterval * 1000).CopyTo(keeyAliveOptionValues, Marshal.SizeOf(dummy) * 2);

            sendTimeout = config.SendTimeout;
            recBufferSize = config.RecBufferSize;
            sendBufferSize = config.SendBufferSize;
        }

        protected override ISocketListener CreateListener(ListenerInfo listenerInfo)
        {
            return new TcpAsyncSocketListener(listenerInfo);
        }

        protected IAppSession CreateSession(Socket client, ISocketSession session)
        {
            if (sendTimeout > 0)
                client.SendTimeout = sendTimeout;
            if (recBufferSize > 0)
                client.ReceiveBufferSize = recBufferSize;
            if (sendBufferSize > 0)
                client.SendBufferSize = sendBufferSize;

            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, keeyAliveOptionValues);
            client.NoDelay = true;
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            return AppServer.CreateAppSession(session);
        }
    }
}
