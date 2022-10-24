using MessagePack;

namespace MessagePackSchema.Runtime.Utils
{
    internal static class ListExtensions
    {
        /// <summary>
        /// Wrapper over AddRange method of list to return <paramref name="list"/>
        /// </summary>
        public static List<T> AppendForReader<T>(this List<T> list, ref MessagePackReader reader, Delegates.ActionRef<T> read)
        {
            for(int i = 0; i < list.Capacity; i++)
            {
                T item = read(ref reader);
                list.Add(item);
            }
            return list;
        }
    }
}
