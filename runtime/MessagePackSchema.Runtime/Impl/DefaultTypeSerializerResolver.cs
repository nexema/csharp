using MessagePackSchema.Runtime.Exceptions;

namespace MessagePackSchema.Runtime.Impl
{
    /// <summary>
    /// DEfault <see cref="ITypeSerializerResolver"/> implementation.
    /// </summary>
    internal class DefaultTypeSerializerResolver : ITypeSerializerResolver
    {
        public ITypeSerializer<TType> Resolve<TType>()
        {
            ITypeSerializer serializer = TypeSerializerPool.TypeSerializers[typeof(TType)].Value;
            if (serializer == null)
                throw new InvalidTypeSerializer($"Type serializer for type {typeof(TType)} not found.");

            return (ITypeSerializer<TType>)serializer;
        }
    }
}