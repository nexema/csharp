namespace MessagePackSchema.Runtime
{
    /// <summary>
    /// Provides methods to resolve <see cref="ITypeSerializer{T}"/>s
    /// </summary>
    public interface ITypeSerializerResolver
    {
        /// <summary>
        /// Resolves a type serializer.
        /// </summary>
        /// <typeparam name="TType">The type to look for its <see cref="ITypeSerializer{T}"/>.</typeparam>
        public ITypeSerializer<TType> Resolve<TType>();
    }
}