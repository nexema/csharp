using MessagePack;

namespace MessagePackSchema.Runtime.Utils
{
    internal static class MapExtensions
    {
        /// <summary>
        /// Reads a key and a value from <see cref="MessagePackReader"/> and adds to a map
        /// </summary>
        /// <param name="length">The total count of the items to add.</param>
        /// <param name="reader">The reader from where to read key and value.</param>
        /// <param name="readKey">The function to read the key.</param>
        /// <param name="readValue">The function to read the value.</param>
        public static Dictionary<K, V> AppendForReader<K, V>(
            this Dictionary<K, V> map,
            int length,
            ref MessagePackReader reader,
            Delegates.ActionRef<K> readKey,
            Delegates.ActionRef<V> readValue) where K : notnull
        {
            for(int i = 0; i < length; i++)
            {
                K key = readKey(ref reader);
                V value = readValue(ref reader);

                map[key] = value;
            }

            return map;
        }
    }
}
