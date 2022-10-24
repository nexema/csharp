namespace MessagePackSchema.Runtime.Exceptions
{
    public class InvalidTypeSerializer : Exception
    {
        public InvalidTypeSerializer(string message) : base(message) { }
    }
}