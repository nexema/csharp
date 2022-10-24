using MessagePack;

namespace MessagePackSchema.Runtime.Utils
{
    internal static class Delegates
    {
        internal delegate TOut ActionRef<TOut>(ref MessagePackReader item);
    }
}