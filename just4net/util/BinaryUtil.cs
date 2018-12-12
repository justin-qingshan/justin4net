using System;
using System.Collections.Generic;

namespace just4net.socket.common
{
    public static class BinaryUtil
    {
        public static int IndexOf<T>(this IList<T> source, T target, int pos, int length)
            where T : IEquatable<T>
        {
            for (int i = pos; i < pos + length; i++)
            {
                if (source[i].Equals(target))
                    return i;
            }

            return -1;
        }

        public static int? SearchMark<T> (this IList<T> source, int offset, int length, T[] mark)
            where T : IEquatable<T>
        {
            int parsedLength;
            return SearchMark(source, offset, length, mark, 0, out parsedLength);
        }

        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark, out int parsedLength)
            where T : IEquatable<T>
        {
            return SearchMark(source, offset, length, mark, 0, out parsedLength);
        }

        public static int? SearchMark<T>(this IList<T> source, int offset, int length, T[] mark, int matched)
            where T : IEquatable<T>
        {
            int parsedLength;
            return SearchMark(source, offset, length, mark, matched, out parsedLength);
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
                for (int i = matchCount; i < mark.Length; i++)
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

                matchCount += 1;

                for(int i = matchCount; i < mark.Length; i++)
                {
                    int checkPos = pos + i;
                    if (checkPos > endOffset)
                        return (0 - matchCount);

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
