using MessagePackSchema.Runtime;

namespace MessagePackSchema.Test
{
    public sealed class DefaultType : BaseSchemaType<DefaultType>, ISchemaType<DefaultType>
    {
        #region SchemaDefinition

        private static readonly List<FieldInfo> _fieldset = new()
        {
            new FieldInfo("name", 0, false, FieldValueType.String, null, null),
            new FieldInfo("names", 1, false, FieldValueType.List, null, new List<(Type, FieldValueType)>{new (typeof(string), FieldValueType.String)}),
            new FieldInfo("another", 2, false, FieldValueType.Map, null, new List<(Type, FieldValueType)>{new (typeof(string), FieldValueType.String), new (typeof(bool), FieldValueType.Boolean)}),
            new FieldInfo("myUint16", 3, true, FieldValueType.Uint16, null, null),
        };

        private static readonly List<Func<object>> _defaults = new()
        {
            () => "",
            () => new List<string>(),
            () => new Dictionary<string, bool>(),
            () => default(ushort),
        };

        private readonly SchemaDefinition<DefaultType> _schemaDefinition = new(_fieldset, 4, _defaults);

        protected override SchemaDefinition<DefaultType> SchemaDefinition => _schemaDefinition; 

        #endregion

        #region Properties

        public string Name
        {
            get => GetValue<string>(0)!;
            set => SetValue(0, value);
        }

        public List<string> Names
        {
            get => GetValue<List<string>>(1)!;
            init => SetList(1, value ?? new List<string>());
        }

        public Dictionary<string, bool> Config
        {
            get => GetValue<Dictionary<string, bool>>(2)!;
            init => SetMap(2, value ?? new Dictionary<string, bool>());
        }

        public ushort? MyUint16
        {
            get => GetValue<ushort?>(3);
            set => SetValue(3, value);
        } 

        #endregion

        #region Methods

        public override DefaultType Clone()
            => new() { Name = Name, Names = Names };

        public override int GetHashCode() => (Name, Names).GetHashCode();

        public override bool Equals(DefaultType? obj)
        {
            if (obj is null)
                return false;

            return Name.Equals(obj.Name) && Names.SequenceEqual(obj.Names);
        }

        public override bool Equals(object? obj) => Equals(obj as DefaultType);

        #endregion

        #region Static Methods

        /// <summary>
        /// Deserializes a byte array as a <see cref="DefaultType"/>
        /// </summary>
        /// <param name="buffer">The byte array to deserialize.</param>
        public static DefaultType Deserialize(byte[] buffer)
        {
            var instance = new DefaultType();
            instance.MergeFrom(buffer);

            return instance;
        }

        #endregion
    }
}
