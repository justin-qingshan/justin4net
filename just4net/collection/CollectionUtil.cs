using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace just4net.collection
{
    public static class CollectionUtil
    {
        /// <summary>
        /// get the values of a dictionary
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static V[] GetValues<K, V>(this Dictionary<K, V> self)
        {
            V[] values = new V[self.Count];
            int i = 0;
            foreach (V value in self.Values)
            {
                values[i++] = value;
            }
            return values;
        }


        /// <summary>
        /// Get keys of dictionary.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static K[] GetKeys<K, V>(this Dictionary<K, V> self)
        {
            K[] keys = new K[self.Count];
            int i = 0;
            foreach (K key in self.Keys)
            {
                keys[i++] = key;
            }
            return keys;
        }


        /// <summary>
        /// Convert dictionary's keys to list.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static List<K> ToKeyList<K, V>(this Dictionary<K, V> self)
        {
            List<K> keys = new List<K>();
            foreach (K key in self.Keys)
                keys.Add(key);

            return keys;
        }


        /// <summary>
        /// Convert dictionary's values to list.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static List<V> ToValueList<K, V>(this Dictionary<K, V> self)
        {
            List<V> values = new List<V>();

            foreach (V value in self.Values)
                values.Add(value);
            return values;
        }


        /// <summary>
        /// Traveral dictionary's keys.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <param name="action"></param>
        public static void TraveralKeys<K, V>(this Dictionary<K, V> self, Action<K> action)
        {
            foreach (K key in self.Keys)
                action(key);
        }


        /// <summary>
        /// Traversal dictionary's keys with index.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <param name="action"></param>
        public static void TraveralKeys<K, V>(this Dictionary<K, V> self, Action<K, int> action)
        {
            int i = 0;
            foreach (K key in self.Keys)
                action(key, i++);
        }


        /// <summary>
        /// Traversal dictionary's values.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <param name="action"></param>
        public static void TraveralValues<K, V>(this Dictionary<K, V> self, Action<V> action)
        {
            foreach (V value in self.Values)
                action(value);
        }


        /// <summary>
        /// Traversal dictionary's values with index.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="self"></param>
        /// <param name="action"></param>
        public static void TraveralValues<K, V>(this Dictionary<K, V> self, Action<V, int> action)
        {
            int i = 0;
            foreach (V value in self.Values)
                action(value, i++);
        }


        /// <summary>
        /// Group a collection by specific property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="self"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static List<T>[] Group<T, TKey>(this ICollection<T> self, Func<T, TKey> func)
        {
            Dictionary<TKey, List<T>> dic = self.GroupDic(func);
            List<T>[] cols = new List<T>[dic.Keys.Count];
            int i = 0;
            foreach(List<T> col in dic.Values)
            {
                cols[i++] = col;
            }

            return cols;
        }


        /// <summary>
        /// Group a collection into dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="self"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Dictionary<TKey, List<T>> GroupDic<T, TKey>(this ICollection<T> self, Func<T, TKey> func)
        {
            Dictionary<TKey, List<T>> dic = new Dictionary<TKey, List<T>>();
            foreach (T t in self)
            {
                TKey key = func(t);
                List<T> col;
                if (dic.TryGetValue(key, out col))
                    col.Add(t);
                else
                {
                    col = new List<T>();
                    col.Add(t);
                    dic.Add(key, col);
                }
            }

            return dic;
        }


        /// <summary>
        /// iterate an enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable), $"源{typeof(T).Name}集合对象不可为空！");
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (T item in enumerable)
                action(item);
        }


        /// <summary>
        /// Euqals to ...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool EqualsTo<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            source = source.ToArray();
            target = target.ToArray();

            if (source.Count() != target.Count())
                return false;

            if (!source.Any() && !target.Any())
                return true;

            if (!source.Except(target).Any() && !target.Except(source).Any())
                return true;

            return false;
        }


        /// <summary>
        /// Check enumerable is null or empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                return true;

            return !enumerable.Any();
        }


        /// <summary>
        /// distinct an enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            HashSet<TKey> seenKeys = new HashSet<TKey>();
            return enumerable.Where(item => seenKeys.Add(keySelector(item)));
        }


        /// <summary>
        /// Extension of AddRange.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="enumerable"></param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            enumerable.ForEach(collection.Add);
        }


        /// <summary>
        /// Get datatable of enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            Type currentType = typeof(T);
            PropertyInfo[] properties = currentType.GetProperties();
            DataTable dt = new DataTable();
            foreach(PropertyInfo property in properties)
            {
                dt.Columns.Add(new DataColumn(property.Name));
            }

            T[] array = enumerable.ToArray();
            for(int i = 0; i < array.Length; i++)
            {
                foreach (PropertyInfo property in properties)
                    dt.Rows[i][property.Name] = property.GetValue(array[i]);
            }
            return dt;
        }

        
    }
}
