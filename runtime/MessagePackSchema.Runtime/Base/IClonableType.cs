namespace MessagePackSchema.Runtime
{
    /// <summary>
    /// Provides a method to clone <see cref="ISchemaType"/>s
    /// </summary>
    /// <typeparam name="T">The <see cref="ISchemaType{T}"/> to clone.</typeparam>
    public interface IClonableType<T> where T : ISchemaType<T>
    {
        /// <summary>
        /// Deep clones the current type instance.
        /// </summary>
        T Clone();
    }
}