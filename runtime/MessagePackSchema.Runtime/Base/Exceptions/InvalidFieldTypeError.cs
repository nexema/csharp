namespace MessagePackSchema.Runtime
{
    public class InvalidFieldTypeError : SystemException
    {
        public InvalidFieldTypeError(int fieldIndex, Type valueType, FieldValueType fieldType) 
            : base($"Field {fieldIndex} accepts {fieldType} value types, given: {valueType}")
        {
        }
    }
}