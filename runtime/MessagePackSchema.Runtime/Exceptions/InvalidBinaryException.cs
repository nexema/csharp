namespace MessagePackSchema.Runtime.Exceptions
{
    /// <summary>
    /// An exception that is thrown when the binary that is tried to be deserialized to a type is invalid.
    /// </summary>
    public class InvalidBinaryException : Exception
    {
        public Type Type { get; set; }

        public InvalidBinaryException(Type type, string message) : base($"{message} - Expected type: {message}")
        {
            Type = type;
        }
    }
}