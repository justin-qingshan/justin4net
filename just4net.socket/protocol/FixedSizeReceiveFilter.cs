using just4net.socket.basic;
using System;

namespace just4net.socket.protocol
{
    public abstract class FixedSizeReceiveFilter<TRequestInfo>
        : IReceiveFilter<TRequestInfo>
        where TRequestInfo: IRequestInfo
    {
        private int parsedLength;
        private int size;
        private int originOffset;
        private int offsetDelta;

        public int Size { get { return size; } }

        public int LeftBufferSize { get { return parsedLength; } }

        public int OffsetDelta { get { return offsetDelta; } }

        public virtual IReceiveFilter<TRequestInfo> NextReceiveFilter { get { return null; } }

        protected readonly static TRequestInfo Null = default(TRequestInfo);

        public FilterState State { get; private set; }

        protected FixedSizeReceiveFilter(int size)
        {
            this.size = size;
        }

        void Init(ISocketSession session)
        {
            originOffset = session.OriginRecOffset;
        }

        public virtual TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = parsedLength + length - size;

            // It means that session has received enough size of data if rest is larger than 0.
            if (rest >= 0)
            {
                var requestInfo = ProcessMatchedRequest(readBuffer, offset - parsedLength, size, toBeCopied);
                InternalReset();
                return requestInfo;
            }
            else
            {
                parsedLength += length;
                offsetDelta = parsedLength;
                rest = 0;

                int expectedOffset = offset + length;
                int newOffset = originOffset + offsetDelta;
                if (newOffset < expectedOffset)
                {
                    Buffer.BlockCopy(readBuffer, offset - parsedLength + length, readBuffer, originOffset, parsedLength);
                }

                return Null;
            }
        }

        /// <summary>
        /// Filters the buffer after the session receive enough size of data.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length</param>
        /// <param name="toByCopied"></param>
        /// <returns></returns>
        protected abstract TRequestInfo ProcessMatchedRequest(byte[] buffer, int offset, int length, bool toByCopied);

        private void InternalReset()
        {
            parsedLength = 0;
            offsetDelta = 0;
        }

        public virtual void Reset()
        {
            InternalReset();
        }
    }
}
