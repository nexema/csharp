namespace MessagePackSchema.Runtime
{
    public class FieldNotAListException : SystemException
    {
        public FieldNotAListException(int fieldIndex) : base($"Field {fieldIndex} is not a list.")
        {
        }
    }

    public class FieldNotAMapException : SystemException
    {
        public FieldNotAMapException(int fieldIndex) : base($"Field {fieldIndex} is not a map.")
        {
        }
    }
}