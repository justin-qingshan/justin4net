using System;
using System.Collections.Concurrent;
using System.Threading;

namespace just4net.socket.common
{
    public interface IPoolInfo
    {
        int MinPoolSize { get; }    

        int MaxPoolSize { get; }

        int AvailableItemsCount { get; }

        int TotalItemsCount { get; }
    }

    public interface ISmartPool<T> : IPoolInfo
    {
        void Init(int minPoolSize, int maxPoolSize, ISmartPoolSourceCreator<T> sourceCreator);

        void Push(T item);

        bool TryGet(out T item);
    }

    public class SmartPool<T> : ISmartPool<T>
    {
        private ConcurrentStack<T> globalStack;

        private ISmartPoolSource[] itemsSource;

        private ISmartPoolSourceCreator<T> sourceCreator;

        private int currentSourceCount;

        private int minPoolSize;

        private int maxPoolSize;

        private int totalItemsCount;

        public int MinPoolSize { get { return minPoolSize; } }

        public int MaxPoolSize { get { return maxPoolSize; } }

        public int AvailableItemsCount { get { return globalStack.Count; } }

        public int TotalItemsCount { get { return totalItemsCount; } }

        private int isIncreasing = 0;

        public void Init(int minPoolSize, int maxPoolSize, ISmartPoolSourceCreator<T> sourceCreator)
        {
            this.minPoolSize = minPoolSize;
            this.maxPoolSize = maxPoolSize;
            this.sourceCreator = sourceCreator;
            globalStack = new ConcurrentStack<T>();

            var n = 0;

            if (minPoolSize != maxPoolSize)
            {
                var currentValue = minPoolSize;

                while (true)
                {
                    n++;
                    int thisValue = currentValue * 2;

                    if (thisValue >= maxPoolSize)
                        break;

                    currentValue = thisValue;
                }
            }

            itemsSource = new ISmartPoolSource[n + 1];
            T[] items;
            itemsSource[0] = sourceCreator.Create(minPoolSize, out items);
            currentSourceCount = 1;

            for (int i = 0; i < items.Length; i++)
                globalStack.Push(items[i]);

            totalItemsCount = minPoolSize;
        }


        public void Push(T item)
        {
            globalStack.Push(item);
        }

        bool TryPopWithWait(out T item, int waitTicks)
        {
            var spinWait = new SpinWait();

            while (true)
            {
                spinWait.SpinOnce();

                if (globalStack.TryPop(out item))
                    return true;

                if (spinWait.Count >= waitTicks)
                    return false;
            }
        }

        public bool TryGet(out T item)
        {
            if (globalStack.TryPop(out item))
                return true;

            int cSourceCount = currentSourceCount;

            if (cSourceCount >= itemsSource.Length)
            {
                return TryPopWithWait(out item, 100);
            }

            int increasing = isIncreasing;

            if (isIncreasing == 1)
                return TryPopWithWait(out item, 100);

            if (Interlocked.CompareExchange(ref isIncreasing, 1, increasing) != increasing)
                return TryPopWithWait(out item, 100);

            IncreaseCapacity();

            isIncreasing = 0;
            if (!globalStack.TryPop(out item))
                return false;

            return true;
        }

        private void IncreaseCapacity()
        {
            var newItemsCount = Math.Min(totalItemsCount, maxPoolSize - totalItemsCount);

            T[] items;
            itemsSource[currentSourceCount++] = sourceCreator.Create(newItemsCount, out items);
            totalItemsCount += newItemsCount;
            for (int i = 0; i < items.Length; i++)
                globalStack.Push(items[i]);
        }

    }
}
