using System;

namespace just4net.socket.common
{
    public class SmartPoolSource : ISmartPoolSource
    {
        public object Source { get; set; }
        public int Count { get; set; }

        public SmartPoolSource(object source, int itemsCount)
        {
            Source = source;
            Count = itemsCount;
        }
    }

    public interface ISmartPoolSource
    {
        int Count { get; }
    }

    public interface ISmartPoolSourceCreator<T>
    {
        ISmartPoolSource Create(int size, out T[] poolItems);
    }

    public class SendingQueueSourceCreator : ISmartPoolSourceCreator<SendingQueue>
    {
        private int sendingQueueSize;

        public SendingQueueSourceCreator(int sendingQueueSize)
        {
            this.sendingQueueSize = sendingQueueSize;
        }

        public ISmartPoolSource Create(int size, out SendingQueue[] poolItems)
        {
            var source = new ArraySegment<byte>[size * sendingQueueSize];
            poolItems = new SendingQueue[size];
            for(int i = 0; i < size; i++)
            {
                poolItems[i] = new SendingQueue(source, i + sendingQueueSize, sendingQueueSize);
            }
            return new SmartPoolSource(source, size);
        }
    }
}
