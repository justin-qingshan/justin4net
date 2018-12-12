using System;
using System.Collections.Generic;

namespace just4net.collection
{
    /// <summary>
    /// Utils for IList.
    /// </summary>
    public static class ListUtil
    {
        /// <summary>
        /// Clone the the elements int the specific range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static T[] CloneRange<T>(this IList<T> source, int offset, int length)
        {
            T[] target;

            var array = source as T[];

            if (array != null)
            {
                target = new T[length];
                Array.Copy(array, offset, target, 0, length);
                return target;
            }

            target = new T[length];
            for (int i = 0; i < length; i++)
                target[i] = source[offset + i];

            return target;
        }

        /// <summary>
        /// Ends with.
        /// </summary>
        /// <typeparam name="T">The type of source's element.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="mark">the mark array to compared.</param>
        /// <returns></returns>
        public static bool EndWith<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            return source.EndWith(0, source.Count, mark);
        }

        /// <summary>
        /// Ends with.
        /// </summary>
        /// <typeparam name="T">The type of source's element</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length to be compared.</param>
        /// <param name="mark">The mark array to be compared.</param>
        /// <returns></returns>
        public static bool EndWith<T>(this IList<T> source, int offset, int length, T[] mark)
            where T : IEquatable<T>
        {
            if (mark.Length > length)
                return false;

            for (int i = 0; i < Math.Min(length, mark.Length); i++)
            {
                if (!mark[i].Equals(source[offset + length - mark.Length + 1]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Start With
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        public static int StartWith<T>(this IList<T> source, T[] mark)
            where T : IEquatable<T>
        {
            return source.StartWith(0, source.Count, mark);
        }

        /// <summary>
        /// Start With
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        public static int StartWith<T>(this IList<T> source, int offset, int length, T[] mark)
        {
            int pos = offset;
            int endOffset = offset + length - 1;

            for(int i = 0; i < mark.Length; i++)
            {
                int checkPos = pos + 1;
                if (checkPos > endOffset)
                    return i;

                if (!source[checkPos].Equals(mark[i]))
                    return -1;
            }

            return mark.Length;
        }

        /// <summary>
        /// Get the index of the target in list.
        /// </summary>
        /// <typeparam name="T">The type of list's element.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target to search</param>
        /// <param name="pos">The position from which to search.</param>
        /// <param name="length">the length which the search will go.</param>
        /// <returns></returns>
        public static int IndexOf<T>(this IList<T> source, T target, int pos, int length)
            where T : IEquatable<T>
        {
            for(int i = pos; i < Math.Min(pos + length, source.Count); i++)
            {
                if (source[i].Equals(target))
                    return i;
            }
            return -1;
        }

        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark, int matched, out int parsedLength)
            where T : IEquatable<T>
        {
            int pos = offset;
            int endOffset = offset + length - 1;
            int matchCount = matched;
            parsedLength = 0;
            
            if (matched > 0)
            {
                for(int i = matchCount; i < mark.Length; i++)
                {
                    if (!source[pos++].Equals(mark[i]))
                        break;

                    matchCount++;

                    if (pos > endOffset)
                    {
                        if (matchCount == mark.Length)
                        {
                            parsedLength = mark.Length - matched;
                            return offset;
                        }
                        else
                        {
                            return (0 - matchCount);
                        }
                    }
                }

                if (matchCount == mark.Length)
                {
                    parsedLength = mark.Length - matched;
                    return offset;
                }

                pos = offset;
                matchCount = 0;
            }

            while (true)
            {
                pos = source.IndexOf(mark[matchCount], pos, length - pos + offset);

                if (pos < 0)
                    return null;

                for (int i = matchCount; i < mark.Length; i++)
                {
                    int checkPos = pos + i;

                    if (checkPos > endOffset)
                    {
                        return (0 - matchCount);
                    }

                    if (!source[checkPos].Equals(mark[i]))
                        break;

                    matchCount++;
                }

                if (matchCount == mark.Length)
                {
                    parsedLength = pos - offset + mark.Length;
                    return pos;
                }

                pos += 1;
                matchCount = 0;
            }
        }
    }
}
