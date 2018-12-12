using just4net.collection;
using just4net.socket.common;
using System;

namespace just4net.socket.protocol
{
    public abstract class FixedHeaderReceiveFilter<TRequestInfo>
        : FixedSizeReceiveFilter<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        private bool headerFound = false;
        private ArraySegment<byte> header;
        private int bodyLength;
        private ArraySegmentList bodyBuffer;

        protected FixedHeaderReceiveFilter(int headerSize)
            : base(headerSize)
        {

        }

        public override TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            if (!headerFound)
                return base.Filter(readBuffer, offset, length, toBeCopied, out rest);

            if (bodyBuffer == null || bodyBuffer.Count == 0)
            {
                if (length < bodyLength)
                {
                    if (bodyBuffer == null)
                        bodyBuffer = new ArraySegmentList();

                    bodyBuffer.AddSegment(readBuffer, offset, length, toBeCopied);
                    rest = 0;
                    return Null;
                }
                else if (length == bodyLength)
                {
                    rest = 0;
                    headerFound = false;
                    return ResolveRequestInfo(header, readBuffer, offset, length);
                }
                else
                {
                    rest = length - bodyLength;
                    headerFound = false;
                    return ResolveRequestInfo(header, readBuffer, offset, bodyLength);
                }
            }
            else
            {
                int required = bodyLength - bodyBuffer.Count;
                if (length < required)
                {
                    bodyBuffer.AddSegment(readBuffer, offset, length, toBeCopied);
                    rest = 0;
                    return Null;
                }
                else if (length == required)
                {
                    bodyBuffer.AddSegment(readBuffer, offset, length, toBeCopied);
                    rest = 0;
                    headerFound = false;
                    var reqeustInfo = ResolveRequestInfo(header, bodyBuffer.ToArrayData());
                    bodyBuffer.ClearSegments();
                    return reqeustInfo;
                }
                else
                {
                    bodyBuffer.AddSegment(readBuffer, offset, required, toBeCopied);
                    rest = length - required;
                    headerFound = false;
                    var requestInfo = ResolveRequestInfo(header, bodyBuffer.ToArrayData(0, bodyLength));
                    bodyBuffer.ClearSegments();
                    return requestInfo;
                }
            }
        }

        protected override TRequestInfo ProcessMatchedRequest(byte[] buffer, int offset, int length, bool toByCopied)
        {
            headerFound = true;
            bodyLength = GetBodyLengthFromHeader(buffer, offset, Size);

            if (toByCopied)
                header = new ArraySegment<byte>(buffer.CloneRange(offset, Size));
            else
                header = new ArraySegment<byte>(buffer, offset, Size);

            if (bodyLength > 0)
                return Null;

            headerFound = false;
            return ResolveRequestInfo(header, null, 0, 0);
        }

        private TRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer)
        {
            return ResolveRequestInfo(header, bodyBuffer, 0, bodyBuffer.Length);
        }

        protected abstract int GetBodyLengthFromHeader(byte[] header, int offset, int length);

        protected abstract TRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int lenth);

        public override void Reset()
        {
            base.Reset();
            headerFound = false;
            bodyLength = 0;
        }

    }
}
