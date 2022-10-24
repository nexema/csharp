using MessagePackSchema.Runtime.Exceptions;

namespace MessagePackSchema.Runtime
{
    internal static class Preconditions
    {
        public static void ThrowInvalidBinary(Type type)
            => throw new InvalidBinaryException(type, "Binary could not be deserialized.");
    }
}