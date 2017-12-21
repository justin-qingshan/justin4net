using System;
using System.Data;

namespace just4net.util
{
    public static class DataConvert
    {
        /// <summary>
        /// Get the value of DataRow[index].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="index"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static T Value<T>(this DataRow row, int index, T defaultValue)
        {
            if (row == null)
                throw new NullReferenceException("row cannot be null.");

            if (index < 0 || index >= row.ItemArray.Length)
                throw new IndexOutOfRangeException("current index " + index + 
                    " is not between 0 and max index with " + row.ItemArray.Length + " of this row");

            if (row[index] == DBNull.Value)
                return defaultValue;
            else
            {
                try { return row.Field<T>(index); }
                catch (Exception ex)
                {
                    string msg = "index=" + index
                        + ", defaultValue=" + defaultValue
                        + ", value=" + row[index].ToString()
                        + ", type=" + row[index].GetType();
                    throw new InvalidCastException(msg, ex);
                }
            }
        }

        /// <summary>
        /// Get the value of DataRow[columnName].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T Value<T>(this DataRow row, int index)
        {
            return row.Value(index, default(T));
        }

        /// <summary>
        /// Get the value of DataRow[columnName].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        public static T Value<T>(this DataRow row, string columnName, T defaultValue)
        {
            if (row[columnName] == DBNull.Value)
                return defaultValue;
            else
            {
                try
                {
                    return row.Field<T>(columnName);
                }
                catch (IndexOutOfRangeException ex)
                {
                    string msg = "column name of " + columnName + " is not exists";
                    throw new IndexOutOfRangeException(msg, ex);
                }
                catch (Exception ex)
                {
                    string msg = "columnName=" + columnName
                        + ", value=" + row[columnName].ToString()
                        + ", type=" + row[columnName].GetType();
                    throw new InvalidCastException(msg, ex);
                }
            }
        }


        /// <summary>
        /// Get the value of DataRow[columnName].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static T Value<T>(this DataRow row, string columnName)
        {
            return row.Value(columnName, default(T));
        }


        /// <summary>
        /// Cast string to datetime. if cannot cast, return the default time.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultTime"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static DateTime ToDateTime(this string str, DateTime defaultTime)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException("string value cannot be null.");

            DateTime time;
            try { time = Convert.ToDateTime(str); }
            catch { time = defaultTime; }

            return time;
        }


        /// <summary>
        /// Convert string value to datetime. if failed return DateTime.MinValue.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string str)
        {
            return str.ToDateTime(DateTime.MinValue);
        }


        /// <summary>
        /// Convert string value to integer. If failed return default integer.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToInt(this string str, int defaultValue)
        {
            int value;
            try { value = Convert.ToInt32(str); }
            catch { value = defaultValue; }
            return value;
        }


        /// <summary>
        /// Convert to Hex string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ToHexString(this int value, int length)
        {
            string hex = Convert.ToString(value, 16).PadLeft(length, '0');
            return hex;
        }


        /// <summary>
        /// Get the universal time of the specific time.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int TimeStamp(this DateTime self)
        {
            TimeSpan span = self.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (int)span.TotalSeconds;
        }

    }
}
