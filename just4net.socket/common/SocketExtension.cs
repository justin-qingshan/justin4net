using System.Net.Sockets;

namespace just4net.socket.common
{
    public static class SocketExtension
    {
        public static void SafeClose(this Socket source)
        {
            if (source == null)
                return;

            try
            {
                if (source.Connected)
                    source.Shutdown(SocketShutdown.Both);
            }
            catch { }

            try { source.Close(); }
            catch { }
        }

        public static void SendData(this Socket source, byte[] data)
        {
            SendData(source, data, 0, data.Length);
        }

        public static void SendData(this Socket source, byte[] data, int offset, int length)
        {
            int sent = 0;
            int thisSent = 0;

            while((length - sent) > 0)
            {
                thisSent = source.Send(data, offset + sent, length - sent, SocketFlags.None);
                sent += thisSent;
            }
        }
    }
}
