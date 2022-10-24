namespace MessagePackSchema.Runtime
{
    public class UnknownFieldException : SystemException
    {
        public UnknownFieldException(int fieldIndex, Exception inner) : base($"Unknown field with index {fieldIndex}.", inner) { }
        public UnknownFieldException(int fieldIndex) : base($"Unknown field with index {fieldIndex}.") { }
    }
}
