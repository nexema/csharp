namespace MessagePackSchema.Runtime
{
    /// <summary>
    /// Contains the basic type information of a schema type.
    /// </summary>
    public interface ISchemaType
    {
        /// <summary>
        /// Serializes the current type to an <see cref="Memory{byte}"/>.
        /// </summary>
        /// <param name="output">The memory to write the output to.</param>
        byte[] Serialize();

        /// <summary>
        /// Serializes the current type to an <see cref="Memory{byte}"/> asynchronously.
        /// </summary>
        /// <param name="output">The memory to write the output to.</param>
        Task<byte[]> SerializeAsync();

        /// <summary>
        /// Deserializes a <see cref="ReadOnlyMemory{byte}"/> and merges to the current type.
        /// </summary>
        /// <param name="input">The input to deserialize.</param>
        void MergeFrom(ReadOnlyMemory<byte> input);

        /// <summary>
        /// Deserializes a <see cref="ReadOnlyMemory{byte}"/> and merges to the current type asynchronously.
        /// </summary>
        /// <param name="input">The input to deserialize.</param>
        Task MergeFromAsync(ReadOnlyMemory<byte> input);
    }

    /// <summary>
    /// Generic implementation of <see cref="ISchemaType"/>.
    /// </summary>
    public interface ISchemaType<T> : ISchemaType, IEquatable<T>, IClonableType<T> where T : ISchemaType<T>
    {
        /// <summary>
        /// Merges the current type instance with other instance.
        /// </summary>
        /// <param name="other">The instance to merge with this one.</param>
        void MergeUsing(T other);
    }
}