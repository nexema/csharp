namespace MessagePackSchema.Runtime
{
    /// <summary>
    /// Contains information about a type's field
    /// </summary>
    public class FieldInfo
    {
        private readonly string m_Name;
        private readonly int m_Index;
        private readonly bool m_Nullable;
        private readonly FieldValueType m_ValueType;
        private readonly Type? m_CustomTypeInfo;
        private readonly List<(Type type, FieldValueType valueType)>? m_TypeArguments;

        public string Name => m_Name;
        
        public int Index => m_Index;
        
        public bool Nullable => m_Nullable;

        public FieldValueType ValueType => m_ValueType;

        public Type? CustomTypeInfo => m_CustomTypeInfo;

        public List<(Type Type, FieldValueType ValueType)>? TypeArguments => m_TypeArguments;


        public FieldInfo(string name, int index, bool nullable, FieldValueType valueType, Type? customTypeInfo, List<(Type type, FieldValueType valueType)>? typeArguments)
        {
            m_Name = name;
            m_Index = index;
            m_Nullable = nullable;
            m_ValueType = valueType;
            m_CustomTypeInfo = customTypeInfo;
            m_TypeArguments = typeArguments;
        }

        public override string ToString() => $"{Name}{(Nullable ? "?" : "")} ({Index})";
    }
}
