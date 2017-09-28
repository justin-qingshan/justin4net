﻿using System;
using System.Collections.Generic;

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


    }
}
