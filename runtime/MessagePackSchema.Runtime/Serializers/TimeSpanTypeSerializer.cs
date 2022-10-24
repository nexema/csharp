using MessagePack;

namespace MessagePackSchema.Runtime.Serializers
{
    internal class TimeSpanTypeSerializer : ITypeSerializer<TimeSpan>
    {
        public TimeSpan Deserialize(ref MessagePackReader reader, ITypeSerializerResolver resolver)
        {
            try
            {
                long seconds = reader.ReadInt64();
                return TimeSpan.FromMilliseconds(seconds);
            }
            catch
            {
                Preconditions.ThrowInvalidBinary(typeof(DateTime));
                return TimeSpan.MinValue;
            }
        }

        public void Serialize(TimeSpan value, ref MessagePackWriter writer, ITypeSerializerResolver resolver)
        {
            long milliseconds = (long)Math.Truncate(value.TotalMilliseconds);
            writer.Write(milliseconds);
        }
    }
}