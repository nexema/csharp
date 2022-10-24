namespace MessagePackSchema.Runtime
{
    public class InvalidFieldTypeException : SystemException
    {
        public InvalidFieldTypeException(int fieldIndex, Type toError, FieldValueType realValueType, Exception? innerException) 
            : base($"Field {fieldIndex} of type {realValueType} cannot be converted to {toError}.", innerException)
        {
        }
    }
}