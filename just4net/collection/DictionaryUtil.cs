using System.Collections.Generic;

namespace just4net.collection
{
    /// <summary>
    /// Extension functions for <seealso cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class DictionaryUtil
    {
        /// <summary>
        /// Get the value by key.
        /// <para>Return default <typeparamref name="V"/> when didn't find the key.</para>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static V GetValue<K, V>(this IDictionary<K, V> source, K key)
        {
            return GetValue(source, key, default(V));
        }

        /// <summary>
        /// Get the value by key.
        /// </summary>
        /// <typeparam name="K">Key type.</typeparam>
        /// <typeparam name="V">Value type.</typeparam>
        /// <param name="source">Source dictionary.</param>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value returned when source doesn't contain the key.</param>
        /// <returns></returns>
        public static V GetValue<K, V>(this IDictionary<K, V> source, K key, V defaultValue)
        {
            V value;
            if (!source.TryGetValue(key, out value))
                value = defaultValue;

            return value;
        }
    }
}
