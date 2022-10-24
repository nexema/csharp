namespace MessagePackSchema.Runtime
{
    /// <summary>
    /// Contains the default of an schema type an used to lookup values
    /// </summary>
    public class SchemaDefinition<T> where T : ISchemaType<T>
    {
        private readonly List<FieldInfo> m_Fields;
        private readonly List<Func<object>> m_Defaults;
        private readonly object?[] m_Values;
        private readonly bool[] m_SetFields; // Maintain a list of all the fields that has been set at least one time

        /// <summary>
        /// The list of fields in the schema
        /// </summary>
        public List<FieldInfo> Fields => m_Fields;

        public SchemaDefinition(List<FieldInfo> fields, int fieldCount, List<Func<object>> defaults)
        {
            m_Fields = fields;
            m_Values = new object?[fieldCount];
            m_SetFields = new bool[fieldCount];
            m_Defaults = defaults;
        }

        /// <summary>
        /// Gets a field value
        /// </summary>
        /// <param name="index">The index of the field</param>
        public object? GetFieldValue(int index) => m_Values[index];

        /// <summary>
        /// Sets a field value
        /// </summary>
        /// <param name="index">The index of the field.</param>
        /// <param name="value">The value to set.</param>
        public void SetFieldValue(int index, object? value)
        {
            m_Values[index] = value;
            m_SetFields[index] = true;
        }

        /// <summary>
        /// Sets the values of the current schema definition
        /// </summary>
        /// <param name="values">The list of values to set.</param>
        public void SetValues(object?[] values)
        {
            var index = 0;
            foreach (var value in values)
            {
                m_Values[index] = value;
                m_SetFields[index] = true;
            }
        }

        /// <summary>
        /// Returns all the values in this schema definition 
        /// </summary>
        public object?[] GetValues() => m_Values;

        /// <summary>
        /// Returns a boolean that indicates if the field has been set at least one time or not
        /// </summary>
        /// <param name="fieldIndex">The index of the field.</param>
        public bool IsFieldSet(int fieldIndex) => m_SetFields[fieldIndex];

        /// <summary>
        /// Returns the default value of a field
        /// </summary>
        /// <param name="fieldIndex">The index of the field.</param>
        public object GetDefaultValue(int fieldIndex) => m_Defaults[fieldIndex]();
    }
}