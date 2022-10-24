using MessagePack;

namespace MessagePackSchema.Runtime.Serializers
{
    internal class DateTimeOffsetTypeSerializer : ITypeSerializer<DateTimeOffset>
    {
        public DateTimeOffset Deserialize(ref MessagePackReader reader, ITypeSerializerResolver resolver)
        {
            int arrayCount = reader.ReadArrayHeader();
            if (arrayCount != 2)
                Preconditions.ThrowInvalidBinary(typeof(DateTimeOffset));

            try
            {
                long seconds = reader.ReadInt64();
                int offsetSeconds = reader.ReadInt32();

                return DateTimeOffset.FromUnixTimeSeconds(seconds).ToOffset(TimeSpan.FromSeconds(offsetSeconds));
            }
            catch
            {
                Preconditions.ThrowInvalidBinary(typeof(DateTimeOffset));
                return DateTime.MinValue;
            }
        }

        public void Serialize(DateTimeOffset value, ref MessagePackWriter writer, ITypeSerializerResolver resolver)
        {
            long seconds = value.ToUnixTimeSeconds();
            int offsetSeconds = (int)Math.Truncate(value.Offset.TotalSeconds);

            writer.WriteArrayHeader(2);
            writer.Write(seconds);
            writer.Write(offsetSeconds);
        }
    }
}