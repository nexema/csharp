using MessagePack;

namespace MessagePackSchema.Runtime.Serializers
{
    internal class DateTimeTypeSerializer : ITypeSerializer<DateTime>
    {
        public DateTime Deserialize(ref MessagePackReader reader, ITypeSerializerResolver resolver)
        {
            int arrayCount = reader.ReadArrayHeader();
            if (arrayCount != 2)
                Preconditions.ThrowInvalidBinary(typeof(DateTime));

            try
            {
                long seconds = reader.ReadInt64();
                _ = reader.ReadInt32();

                return DateTime.UnixEpoch.AddSeconds(seconds);
            }
            catch
            {
                Preconditions.ThrowInvalidBinary(typeof(DateTime));
                return DateTime.MinValue;
            }
        }

        public void Serialize(DateTime value, ref MessagePackWriter writer, ITypeSerializerResolver resolver)
        {
            DateTime utc = value.ToUniversalTime();
            long seconds = (long)Math.Truncate(utc.Subtract(DateTime.UnixEpoch).TotalSeconds);

            writer.WriteArrayHeader(2);
            writer.Write(seconds);
            writer.Write(0);
        }
    }
}