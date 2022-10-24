using MessagePack;

namespace MessagePackSchema.Runtime.Serializers
{
    public class NullableTypeSerializer<T> : ITypeSerializer<T?> where T : struct
    {
        public T? Deserialize(ref MessagePackReader reader, ITypeSerializerResolver resolver)
        {
            if (reader.TryReadNil())
                return null;

            return resolver.Resolve<T>().Deserialize(ref reader, resolver);
        }

        public void Serialize(T? value, ref MessagePackWriter writer, ITypeSerializerResolver resolver)
        {
            if (value == null)
                writer.WriteNil();
            else
                resolver.Resolve<T>().Serialize(value.Value, ref writer, resolver);
        }
    }
}