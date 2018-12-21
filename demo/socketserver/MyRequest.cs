using just4net.socket.protocol;
using System;
using System.Text;

namespace demo.socketserver
{
    public class MyRequest : IRequestInfo<MyRequestHeader, MyRequestBody>
    {
        public MyRequestBody Body { get; set; }

        public MyRequestHeader Header { get; set; }


        public string Key
        {
            get { return ""; }
        }

        public byte[] Bytes
        {
            get
            {
                byte[] bytes = new byte[MyRequestHeader.LENGTH + Header.Length];
                Array.Copy(BitConverter.GetBytes(Header.Length), 0, bytes, 0, 4);
                Array.Copy(BitConverter.GetBytes(Header.MainType), 0, bytes, 4, 2);
                Array.Copy(BitConverter.GetBytes(Header.SubType), 0, bytes, 6, 2);
                Array.Copy(BitConverter.GetBytes(Header.Version), 0, bytes, 8, 4);
                Array.Copy(BitConverter.GetBytes(Header.Flag), 0, bytes, 12, 4);
                Array.Copy(BitConverter.GetBytes(Header.Id), 0, bytes, 16, 4);
                Array.Copy(BitConverter.GetBytes(Header.Order), 0, bytes, 20, 4);
                Array.Copy(Header.Remain, 0, bytes, 24, 12);
                Array.Copy(Body.Content, 0, bytes, 36, Header.Length);
                return bytes;
            }
        }

        public static MyRequest GenerateMsgs(string str, ushort mainType, ushort subType, uint flag)
        {
            MyRequestHeader header = new MyRequestHeader();
            header.MainType = mainType;
            header.SubType = subType;
            header.Flag = flag;
            header.Version = 1;
            header.Id = GenerateId();
            header.Remain = new byte[12];

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            header.Length = (uint)bytes.Length;

            return new MyRequest
            {
                Header = header,
                Body = new MyRequestBody(bytes)
            };
        }

        private static uint GenerateId()
        {
            DateTime time1 = DateTime.Now;
            DateTime time2 = new DateTime(time1.Year, time1.Month, 1, 0, 0, 0);
            TimeSpan span = time1 - time2;
            uint a = Convert.ToUInt32(span.TotalMilliseconds);
            return a;
        }
    }

    public class MyRequestHeader
    {
        public const int LENGTH = 36;

        public uint Length;          // 4 Byte 消息体长度

        public ushort MainType;     // 2 Byte 主消息类型
        public ushort SubType;      // 2 Byte 子消息类型

        public uint Version;        // 4 Byte 版本
        public uint Flag;           // 4 Byte 标记 （10 - 发送消息，11 - 发送消息有后续， 20 - 回复消息， 21 - 回复消息有后续）

        public uint Id;             // 4 Byte 消息Id

        public uint Order;          // 4 Byte 消息序号，默认为1

        public byte[] Remain;       // 12 Byte 预留
    }

    public class MyRequestBody
    {
        public const int MAX_LENGTH = 8192;

        private byte[] content;
        private Encoding encoding = Encoding.UTF8;

        public MyRequestBody(string str, Encoding encoding)
        {
            this.encoding = encoding;
            content = encoding.GetBytes(str);
            Exception ex = GenerateConstructorEx(str);
            if (ex != null)
                throw ex;
        }


        public MyRequestBody(string str) : this(str, Encoding.UTF8) { }


        public MyRequestBody(byte[] msgBody)
        {
            content = msgBody;
        }


        private Exception GenerateConstructorEx(string str)
        {
            if (content.Length > MAX_LENGTH)
                return new ArgumentException("length of the parameter 'str' can't be more than " + MAX_LENGTH
                    + ", it's " + content.Length + " of '" + str + "'");

            return null;
        }


        public int Length
        {
            get { return content.Length; }
        }


        /// <summary>
        /// 消息体内容（字节数组）
        /// </summary>
        public byte[] Content
        {
            get { return content; }
        }


        /// <summary>
        /// 消息体内容（字符串）
        /// </summary>
        public string ContentStr
        {
            get { return encoding.GetString(content); }
        }
    }
}
