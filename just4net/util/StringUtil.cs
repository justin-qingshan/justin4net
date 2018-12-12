using System.Security.Cryptography;
using System.Text;
using System;

namespace just4net.util
{
    /// <summary>
    /// Extension functions for string.
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        /// Replace the chars of specific string with a new char.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="newChar"></param>
        /// <param name="oldChars"></param>
        /// <returns></returns>
        public static string Replaces(this string self, char newChar, params char[] oldChars)
        {
            foreach (char oldChar in oldChars)
            {
                self = self.Replace(oldChar, newChar);
            }
            return self;
        }

        /// <summary>
        /// Replace the string values in specific string with a new string value.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="newValue"></param>
        /// <param name="oldValues"></param>
        /// <returns></returns>
        public static string Replaces(this string self, string newValue, params string[] oldValues)
        {
            foreach (string oldValue in oldValues)
            {
                self = self.Replace(oldValue, newValue);
            }
            return self;
        }

        /// <summary>
        /// Remove the string values in specific string.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public static string Remove(this string self, params string[] toRemove)
        {
            return self.Replaces("", toRemove);
        }

        public static string ToMD5(this string text)
        {
            byte[] buffer = Encoding.Default.GetBytes(text);
            using(MD5 md5 = MD5.Create())
            {
                buffer = md5.ComputeHash(buffer);
                StringBuilder md5Builder = new StringBuilder();
                foreach(byte @byte in buffer)
                {
                    md5Builder.Append(@byte.ToString("x2"));
                }
                return md5Builder.ToString();
            }

        }

        /// <summary>
        /// Euqals when ignore case.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Eq(this string source, string target, bool ignoreCase = true)
        {
            if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(target))
                return true;

            if (ignoreCase)
                return string.Equals(source, target, StringComparison.CurrentCultureIgnoreCase);
            else
                return string.Equals(source, target, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Convert string to integer.
        /// <para>Return 0 if convert failed.</para>
        /// </summary>
        /// <param name="source">source string</param>
        /// <returns></returns>
        public static int ToInt32(this string source)
        {
            return ToInt32(source, 0);
        }

        /// <summary>
        /// Convert string to integer.
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="defaultValue">default value returned when convert failed.</param>
        /// <returns></returns>
        public static int ToInt32(this string source, int defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            int value;
            if (!int.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to long.
        /// <para>Return 0 if convert failed.</para>
        /// </summary>
        /// <param name="source">source string.</param>
        /// <returns></returns>
        public static long ToLong(this string source)
        {
            return ToLong(source, 0L);
        }

        /// <summary>
        /// Convert string to long.
        /// </summary>
        /// <param name="source">source string.</param>
        /// <param name="defaultValue">default value returned when convert failed.</param>
        /// <returns></returns>
        public static long ToLong(this string source, long defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            long value;

            if (!long.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert to string to short.
        /// <para>return 0 when convert failed.</para>
        /// </summary>
        /// <param name="source">source string.</param>
        /// <returns></returns>
        public static short ToShort(this string source)
        {
            return ToShort(source, 0);
        }

        /// <summary>
        /// Convert to string to short.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="defaultValue">Default value returned when convert failed.</param>
        /// <returns></returns>
        public static short ToShort(this string source, short defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            short value;
            if (!short.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to decimal.
        /// <para>Return 0 if convert failed.</para>
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns></returns>
        public static decimal ToDecimal(this string source)
        {
            return ToDecimal(source, 0);
        }

        /// <summary>
        /// Convert string to decimal.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="defaultValue">Default value returned when convert failed.</param>
        /// <returns></returns>
        public static decimal ToDecimal(this string source, decimal defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;
            decimal value;
            if (!decimal.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to boolean.
        /// <para>return <seealso cref="false"/> if convert failed.</para>
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns></returns>
        public static bool ToBoolean(this string source)
        {
            return ToBoolean(source, false);
        }

        /// <summary>
        /// Convert string to boolean.
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="defaultValue">Default value returned when convert failed.</param>
        /// <returns></returns>
        public static bool ToBoolean(this string source, bool defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            bool value;

            if (!bool.TryParse(source, out value))
                value = defaultValue;

            return value;
        }
    }
}
