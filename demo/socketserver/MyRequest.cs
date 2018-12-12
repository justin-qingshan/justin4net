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
