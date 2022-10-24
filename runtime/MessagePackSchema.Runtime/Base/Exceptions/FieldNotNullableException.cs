namespace MessagePackSchema.Runtime
{
    public class FieldNotNullableException : SystemException
    {
        public FieldNotNullableException(int fieldIndex) : base($"Trying to assign a null value to a field ({fieldIndex}) which is not nullable.")
        {
        }
    }
}