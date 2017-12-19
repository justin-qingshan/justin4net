using System;

namespace just4net.util
{
    public static class DataCompare
    {
        /// <summary>
        /// Compare integer with other integers.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool Equalss(this int self, params int[] values)
        {
            foreach (int value in values)
            {
                if (self == value)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Compare string value with other string values.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool Equalss(this string self, params string[] values)
        {
            if (string.IsNullOrEmpty(self))
                throw new ArgumentNullException("self cannot be null.");

            foreach (string value in values)
            {
                if (self.Equals(value))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Compare string value with other string values.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="comparison"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool Equalss(this string self, StringComparison comparison, params string[] values)
        {
            if (string.IsNullOrEmpty(self))
                throw new ArgumentNullException("self cannot be null.");

            foreach (string value in values)
            {
                if (self.Equals(value, comparison))
                    return true;
            }
            return false;
        }
    }
}
