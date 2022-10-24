namespace MessagePackSchema.Runtime
{
    public class InvalidCustomTypeException : SystemException
    {
        public InvalidCustomTypeException(int fieldIndex, Type fieldType, Type setType) 
            : base($"Cannot set value of type {setType} to field {fieldIndex} which accepts values of type {fieldType}.")
        {
        }
    }
}