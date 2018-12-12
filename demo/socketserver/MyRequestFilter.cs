using just4net.socket.protocol;
using System;
using just4net.socket.basic;
using System.Net;

namespace demo.socketserver
{
    public class MyReceiveFilterFactory : IReceiveFilterFactory<MyRequest>
    {
        public IReceiveFilter<MyRequest> CreateFilter(IAppServer server, IAppSession session, IPEndPoint remoteEndPoint)
        {
            return new MyRequestFilter();
        }
    }

    public class MyRequestFilter : FixedHeaderReceiveFilter<MyRequest>
    {
        public MyRequestFilter() : base(MyRequestHeader.LENGTH)
        {
        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            return (int)BitConverter.ToUInt32(header, offset);
        }

        protected override MyRequest ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            var head = ResolveHeader(header.Array);
            byte[] bytes = new byte[length];
            MyRequestBody body;
            if (length == 0)
                body = new MyRequestBody("");
            else
            {
                Array.Copy(bodyBuffer, offset, bytes, 0, length);
                body = new MyRequestBody(bytes);
            }

            return new MyRequest { Header = head, Body = body };
        }

        private MyRequestHeader ResolveHeader(byte[] headBytes)
        {
            var head = new MyRequestHeader();
            head.Length = BitConverter.ToUInt32(headBytes, 0);
            head.MainType = BitConverter.ToUInt16(headBytes, 4);
            head.SubType = BitConverter.ToUInt16(headBytes, 6);
            head.Version = BitConverter.ToUInt32(headBytes, 8);
            head.Flag = BitConverter.ToUInt32(headBytes, 12);
            head.Id = BitConverter.ToUInt32(headBytes, 16);
            head.Order = BitConverter.ToUInt32(headBytes, 20);
            head.Remain = new byte[12];
            return head;
        }
    }
}
