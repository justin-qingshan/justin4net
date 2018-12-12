using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace just4net.socket.common
{
    public sealed class SendingQueue : IList<ArraySegment<byte>>
    {
        private readonly int offset;

        private readonly int capacity;

        private int currentCount = 0;

        private ArraySegment<byte>[] globalQueue;

        private static ArraySegment<byte> Null = default(ArraySegment<byte>);

        private int updatingCount;

        private bool _readonly = false;

        private ushort trackId = 1;

        private int innerOffset = 0;

        public ushort TrackID { get { return trackId; } }

        public int Capacity { get { return capacity; } }    

        public int Count { get { return currentCount - innerOffset; } } 

        public int Position { get; set; }

        public bool IsReadOnly { get { return _readonly; } }

        public SendingQueue(ArraySegment<byte>[] globalQueue, int offset, int capacity)
        {
            this.globalQueue = globalQueue;
            this.offset = offset;
            this.capacity = capacity;
        }

        public void StartEnqueue()
        {
            _readonly = false;
        }

        public void StopEnqueue()
        {
            if (_readonly)
                return;

            _readonly = true;

            if (updatingCount <= 0)
                return;

            var spinWait = new SpinWait();
            spinWait.SpinOnce();

            // Wait until all enqueues are finished.
            while (updatingCount > 0)
                spinWait.SpinOnce();
        }

        public bool Enqueue(ArraySegment<byte> item, ushort trackId)
        {
            if (_readonly)
                return false;

            Interlocked.Increment(ref updatingCount);
            bool conflict;
            while (!_readonly)
            {
                if (TryEnqueue(item, out conflict, trackId))
                {
                    Interlocked.Decrement(ref updatingCount);
                    return true;
                }

                if (!conflict)
                    break;
            }

            Interlocked.Decrement(ref updatingCount);
            return false;
        }

        private bool TryEnqueue(ArraySegment<byte> item, out bool conflict, ushort trackId)
        {
            conflict = false;

            var oldCount = currentCount;

            if (oldCount >= capacity)
                return false;

            if (_readonly)
                return false;

            if (trackId != this.trackId)
                return false;

            int compareCount = Interlocked.CompareExchange(ref currentCount, oldCount + 1, oldCount);

            if (compareCount != oldCount)
            {
                conflict = true;
                return false;
            }

            globalQueue[offset + oldCount] = item;
            return true;
        }

        public bool Enqueue(IList<ArraySegment<byte>> items, ushort trackId)
        {
            if (_readonly)
                return false;

            Interlocked.Increment(ref updatingCount);

            bool conflict;
            while (!_readonly)
            {
                if (TryEnqueue(items, out conflict, trackId))
                {
                    Interlocked.Decrement(ref updatingCount);
                    return true;
                }

                if (!conflict)
                    break;
            }

            Interlocked.Decrement(ref updatingCount);
            return false;
        }

        private bool TryEnqueue(IList<ArraySegment<byte>> items, out bool conflict, ushort trackId)
        {
            conflict = false;

            int oldCount = currentCount;

            int newItemCount = items.Count;
            int expectedCount = oldCount + newItemCount;

            if (expectedCount > capacity)
                return false;

            if (_readonly)
                return false;

            if (this.trackId != trackId)
                return false;

            int compareCount = Interlocked.CompareExchange(ref currentCount, expectedCount, oldCount);

            if (compareCount != oldCount)
            {
                conflict = true;
                return false;
            }

            var queue = globalQueue;
            for (int i = 0; i < items.Count; i++)
                queue[offset + oldCount + i] = items[i];

            return true;
        }

        public void Clear()
        {
            if (trackId >= ushort.MaxValue)
                trackId = 1;
            else
                trackId++;

            for(int i = 0; i < currentCount; i++)
            {
                globalQueue[offset + i] = Null;
            }

            currentCount = 0;
            innerOffset = 0;
            Position = 0;
        }

        public void InternalTrim(int offset)
        {
            int innerCount = currentCount - innerOffset;
            int subTotal = 0;

            for(int i = innerOffset; i < innerCount; i++)
            {
                var segment = globalQueue[this.offset + i];
                subTotal += segment.Count;

                if (subTotal <= offset)
                    continue;

                innerOffset = i;

                int rest = subTotal - offset;
                globalQueue[this.offset + i] = new ArraySegment<byte>(segment.Array, segment.Offset + segment.Count - rest, rest);

                break;
            }
        }

        public void CopyTo(ArraySegment<byte>[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
                array[arrayIndex + i] = this[i];
        }

        public ArraySegment<byte> this[int index]
        {
            get
            {
                int targetIndex = offset + innerOffset + index;
                var value = globalQueue[targetIndex];

                if (value.Array != null)
                    return value;

                var spinWait = new SpinWait();

                while (true)
                {
                    spinWait.SpinOnce();
                    value = globalQueue[targetIndex];

                    if (value.Array != null)
                        return value;

                    if (spinWait.Count > 50)
                        return value;
                }
            }
            set { throw new NotSupportedException(); }
        }

        public IEnumerator<ArraySegment<byte>> GetEnumerator()
        {
            for (int i = 0; i < (currentCount - innerOffset); i++)
            {
                yield return globalQueue[offset + innerOffset + i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public bool Contains(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void Add(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

    }
}
