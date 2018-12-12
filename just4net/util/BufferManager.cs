using System.Collections.Generic;

namespace just4net.util
{
    /// <summary>
    /// This class creates a single large buffer which can be divided up.
    /// This enables buffers to be easily reused and gaurds against fragmenting heap memory.
    /// <para>
    /// The operations exposed on the BufferManager class are not thread safe.
    /// </para>
    /// </summary>
    public class BufferManager
    {
        int bytesNum;
        byte[] buffer;
        Stack<int> freeIndexPool;
        int currentIndex;
        int bufferSize;

        public byte[] Buffer { get { return buffer; } }

        public int BufferSize { get { return bufferSize; } }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="totalBytes">The total bytes number.</param>
        /// <param name="bufferSize">the size of every single buffer.</param>
        public BufferManager(int totalBytes, int bufferSize)
        {
            bytesNum = totalBytes;
            currentIndex = 0;
            this.bufferSize = bufferSize;
            freeIndexPool = new Stack<int>();
        }

        /// <summary>
        /// Allocates buffer space used by the buffer manager.
        /// </summary>
        public void InitBuffer()
        {
            buffer = new byte[bytesNum];
        }

        /// <summary>
        /// Set a buffer to use.
        /// </summary>
        /// <param name="offset">The offset can be used in the <see cref="Buffer"/></param>
        /// <returns>true if the buffer was successfully set, otherwise false.</returns>
        public bool SetBuffer(out int offset)
        {
            offset = -1;
            if (freeIndexPool.Count > 0)
                offset = freeIndexPool.Pop();
            else
            {
                if ((bytesNum - bufferSize) < currentIndex)
                    return false;

                offset = currentIndex;
                currentIndex += bufferSize;
            }

            return true;
        }
        
        /// <summary>
        /// Free a set of buffer.
        /// </summary>
        /// <param name="offset">the offset of <see cref="Buffer"/> which will be freed.</param>
        public void FreeBuffer(int offset)
        {
            freeIndexPool.Push(offset);
        }
    }
}
