using MessagePack;

namespace MessagePackSchema.Runtime
{
    /// <summary>
    /// Internal empty interface to allow <see cref="ITypeSerializerResolver"/>.
    /// </summary>
    public interface ITypeSerializer { }

    /// <summary>
    /// Provides two methods to serialize and deserialize schema types
    /// </summary>
    /// <typeparam name="T">The <see cref="ISchemaType{T}"/> to serialize/deserialize.</typeparam>
    public interface ITypeSerializer<T> : ITypeSerializer
    {
        /// <summary>
        /// Serializes the current type to a stream.
        /// </summary>
        void Serialize(T value, ref MessagePackWriter writer, ITypeSerializerResolver resolver);

        /// <summary>
        /// Deserailizes a stream into <typeparamref name="T"/>
        /// </summary>
        T Deserialize(ref MessagePackReader reader, ITypeSerializerResolver resolver);
    }
}