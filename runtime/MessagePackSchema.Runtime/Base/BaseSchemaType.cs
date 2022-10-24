using MessagePack;
using MessagePackSchema.Runtime.Utils;
using System.Buffers;
using System.Collections;

namespace MessagePackSchema.Runtime
{
    public abstract class BaseSchemaType<T> : ISchemaType<T> where T : ISchemaType<T>
    {
        protected abstract SchemaDefinition<T> SchemaDefinition { get; }

        #region Constructor

        public BaseSchemaType()
        {

        }

        #endregion

        #region Public Methods

        public abstract bool Equals(T? other);

        public abstract T Clone();

        public void MergeFrom(ReadOnlyMemory<byte> input)
        {
            var reader = new MessagePackReader(input);
            foreach(var field in SchemaDefinition.Fields)
            {
                var value = ReadFieldType(ref reader, field);
                SetValueSkipCheck(field.Index, value);
            }
        }

        public Task MergeFromAsync(ReadOnlyMemory<byte> input)
        {
            return Task.Run(() => MergeFrom(input));
        }

        public void MergeUsing(T other)
        {
            if (other == null)
                throw new NullReferenceException(nameof(other));

            if (other is not BaseSchemaType<T> otherSchema)
                throw new Exception("Cannot merge using other type if T does not inherit from BaseSchemaType.");

            SchemaDefinition.SetValues(otherSchema!.SchemaDefinition.GetValues());
        }

        public byte[] Serialize()
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(bufferWriter);

            foreach(var field in SchemaDefinition.Fields)
            {
                WriteField(ref writer, field, SchemaDefinition.GetFieldValue(field.Index));
            }

            writer.Flush();

            return bufferWriter.WrittenMemory.ToArray();
        }

        public Task<byte[]> SerializeAsync()
        {
            return Task.Run(() => Serialize());
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Retrieves the value of a field
        /// </summary>
        /// <typeparam name="T">The type of the filed</typeparam>
        /// <param name="fieldIndex">The index of the field</param>
        protected V? GetValue<V>(int fieldIndex)
        {
            // get raw value of field
            object? rawValue;
            try
            {
                rawValue = SchemaDefinition.GetFieldValue(fieldIndex);
            }
            catch(Exception ex)
            {
                throw new UnknownFieldException(fieldIndex, ex);
            }
            
            // if null return the default value of V
            if (rawValue == null && !SchemaDefinition.Fields[fieldIndex].Nullable && !SchemaDefinition.IsFieldSet(fieldIndex))
            {
                // set default value for field because its not nullable and maybe it was not set at startup
                rawValue = SchemaDefinition.GetDefaultValue(fieldIndex);
                SetValueSkipCheck(fieldIndex, rawValue);
            }

            try
            {
                // cast value to V
                var realValue = (V?)rawValue;
                return realValue;
            }
            catch(Exception ex)
            {
                throw new InvalidFieldTypeException(fieldIndex, typeof(T), SchemaDefinition.Fields[fieldIndex].ValueType, ex);
            }
        }

        /// <summary>
        /// Sets the value of the field <paramref name="fieldIndex"/> to <paramref name="value"/>
        /// </summary>
        protected void SetValue<V>(int fieldIndex, V? value)
        {
            // Verify if field exists
            FieldInfo? fieldInfo;
            try { fieldInfo = SchemaDefinition.Fields[fieldIndex]; } catch { throw new UnknownFieldException(fieldIndex); }

            if(fieldInfo == null)
                throw new UnknownFieldException(fieldIndex);

            // Verify field nullable
            if (value == null && !fieldInfo.Nullable)
                throw new FieldNotNullableException(fieldIndex);

            // Verify field type
            Type vType = typeof(V);
            VerifyFieldType(fieldInfo, vType);

            // Set value
            try
            {
                SchemaDefinition.SetFieldValue(fieldIndex, value);
            }
            catch(Exception ex)
            {
                throw new Exception("Cannot set value of field.", ex);
            }

        }

        /// <summary>
        /// Sets a list value
        /// </summary>
        protected void SetList<V>(int fieldIndex, List<V>? value)
        {
            // Verify if field exists
            FieldInfo? fieldInfo;
            try { fieldInfo = SchemaDefinition.Fields[fieldIndex]; } catch { throw new UnknownFieldException(fieldIndex); }

            if (fieldInfo == null)
                throw new UnknownFieldException(fieldIndex);

            // Verify field nullability
            if (value == null)
                throw new FieldNotNullableException(fieldIndex);

            // Set value
            VerifyListType(fieldInfo, value!.GetType().GenericTypeArguments[0]);
            try
            {
                SchemaDefinition.SetFieldValue(fieldIndex, value);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot set value of field.", ex);
            }
        }

        /// <summary>
        /// Sets a map value
        /// </summary>
        protected void SetMap<K, V>(int fieldIndex, Dictionary<K, V>? value) where K : notnull
        {
            // Verify if field exists
            FieldInfo? fieldInfo;
            try { fieldInfo = SchemaDefinition.Fields[fieldIndex]; } catch { throw new UnknownFieldException(fieldIndex); }

            if (fieldInfo == null)
                throw new UnknownFieldException(fieldIndex);

            // Verify field nullability
            if (value == null)
                throw new FieldNotNullableException(fieldIndex);

            // Set value
            var genericArguments = value!.GetType().GenericTypeArguments;
            VerifyMapType(fieldInfo, genericArguments[0], genericArguments[1]);
            try
            {
                SchemaDefinition.SetFieldValue(fieldIndex, value);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot set value of field.", ex);
            }
        }

        /// <summary>
        /// Sets the value of a field without doing any check
        /// </summary>
        /// <param name="fieldIndex">The index of the field.</param>
        /// <param name="value">The value to set.</param>
        protected void SetValueSkipCheck(int fieldIndex, object? value)
        {
            SchemaDefinition.SetFieldValue(fieldIndex, value);
        }

        #endregion

        #region Private Static Methods

        #region Write Methods

        private void WriteField(ref MessagePackWriter writer, FieldInfo field, object? value)
        {
            if(!field.Nullable && value == null)
            {
                value = SchemaDefinition.GetDefaultValue(field.Index);
                //throw new ArgumentException($"Field {field.Index} does not accept null values.");
            }

            WriteFieldType(ref writer, field.ValueType, value, field.TypeArguments);
        }

        private static void WriteFieldType(ref MessagePackWriter writer, FieldValueType valueType, object? value, List<(Type type, FieldValueType valueType)>? typeArguments, int deep = 1)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            switch (valueType)
            {
                case FieldValueType.Boolean:
                    writer.Write((bool)value!);
                    break;

                case FieldValueType.String:
                    writer.Write((string)value!);
                    break;

                case FieldValueType.Uint8:
                    writer.Write((byte)value!);
                    break;

                case FieldValueType.Uint16:
                    writer.Write((ushort)value!);
                    break;

                case FieldValueType.Uint32:
                    writer.Write((uint)value!);
                    break;

                case FieldValueType.Uint64:
                    writer.Write((ulong)value!);
                    break;

                case FieldValueType.Int8:
                    writer.Write((sbyte)value!);
                    break;

                case FieldValueType.Int16:
                    writer.Write((short)value!);
                    break;

                case FieldValueType.Int32:
                    writer.Write((int)value!);
                    break;

                case FieldValueType.Int64:
                    writer.Write((long)value!);
                    break;

                case FieldValueType.Float32:
                    writer.Write((float)value!);
                    break;

                case FieldValueType.Float64:
                    writer.Write((double)value!);
                    break;

                case FieldValueType.List:
                    if (deep != 1)
                        throw new ArgumentException("Lists cannot be used as argument for list or map.");

                    WriteList(ref writer, value!, typeArguments![0]);
                    break;

                case FieldValueType.Map:
                    if (deep != 1)
                        throw new ArgumentException("Maps cannot be used as argument for list or map.");

                    WriteMap(ref writer, value!, typeArguments![0].valueType, typeArguments![1]);
                    break;

                case FieldValueType.Custom:
                    throw new NotImplementedException();
            }

        }

        private static void WriteList(ref MessagePackWriter writer, object list, (Type type, FieldValueType valueType) valueType)
        {
            IList clrList = (IList)list;
            int length = clrList.Count;
            var listValueType = valueType.valueType;
            var listValueTypeClrType = valueType.type;

            writer.WriteArrayHeader(length);
            for(int i = 0; i < length; i++)
            {
                object? item = clrList[i];
                WriteFieldType(ref writer, listValueType, item, null, deep: 2);
            }
        }

        private static void WriteMap(ref MessagePackWriter writer, object map, FieldValueType keyType, (Type type, FieldValueType valueType) valueType)
        {
            IDictionary clrMap = (IDictionary)map;
            int length = clrMap.Count;

            writer.WriteMapHeader(length);
            foreach(var key in clrMap.Keys)
            {
                object? item = clrMap[key];
                WriteFieldType(ref writer, keyType, key, null, deep: 2);
                WriteFieldType(ref writer, valueType.valueType, item, null, deep: 2);
            }
        }

        #endregion

        #region Read Methods

        private static object? ReadFieldType(ref MessagePackReader reader, FieldInfo fieldInfo)
        {
            if (fieldInfo.Nullable && reader.TryReadNil())
                return null;

            return fieldInfo.ValueType switch
            {
                FieldValueType.Boolean => reader.ReadBoolean(),
                FieldValueType.String => reader.ReadString(),
                FieldValueType.Uint8 => reader.ReadByte(),
                FieldValueType.Uint16 => reader.ReadUInt16(),
                FieldValueType.Uint32 => reader.ReadUInt32(),
                FieldValueType.Uint64 => reader.ReadUInt64(),
                FieldValueType.Int8 => reader.ReadSByte(),
                FieldValueType.Int16 => reader.ReadInt16(),
                FieldValueType.Int32 => reader.ReadInt32(),
                FieldValueType.Int64 => reader.ReadInt64(),
                FieldValueType.Float32 => reader.ReadSingle(),
                FieldValueType.Float64 => reader.ReadDouble(),
                FieldValueType.Binary => reader.ReadBytes()!.Value,
                FieldValueType.List => ReadList(ref reader, fieldInfo.TypeArguments![0].ValueType),
                FieldValueType.Map => ReadMap(ref reader, fieldInfo.TypeArguments![0].ValueType, fieldInfo.TypeArguments![1].ValueType),
                FieldValueType.Custom => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        private static object ReadMap(ref MessagePackReader reader, FieldValueType keyValueType, FieldValueType valueValueType)
        {
            int length = reader.ReadMapHeader();

            return keyValueType switch
            {
                FieldValueType.String => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<string, string>(length).AppendForReader(length, ref reader, ReadString, ReadString),
                    FieldValueType.Boolean => new Dictionary<string, bool>(length).AppendForReader(length, ref reader, ReadString, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<string, byte>(length).AppendForReader(length, ref reader, ReadString, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<string, ushort>(length).AppendForReader(length, ref reader, ReadString, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<string, uint>(length).AppendForReader(length, ref reader, ReadString, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<string, ulong>(length).AppendForReader(length, ref reader, ReadString, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<string, sbyte>(length).AppendForReader(length, ref reader, ReadString, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<string, short>(length).AppendForReader(length, ref reader, ReadString, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<string, int>(length).AppendForReader(length, ref reader, ReadString, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<string, long>(length).AppendForReader(length, ref reader, ReadString, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<string, float>(length).AppendForReader(length, ref reader, ReadString, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<string, double>(length).AppendForReader(length, ref reader, ReadString, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Boolean => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<bool, string>(length).AppendForReader(length, ref reader, ReadBoolean, ReadString),
                    FieldValueType.Boolean => new Dictionary<bool, bool>(length).AppendForReader(length, ref reader, ReadBoolean, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<bool, byte>(length).AppendForReader(length, ref reader, ReadBoolean, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<bool, ushort>(length).AppendForReader(length, ref reader, ReadBoolean, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<bool, uint>(length).AppendForReader(length, ref reader, ReadBoolean, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<bool, ulong>(length).AppendForReader(length, ref reader, ReadBoolean, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<bool, sbyte>(length).AppendForReader(length, ref reader, ReadBoolean, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<bool, short>(length).AppendForReader(length, ref reader, ReadBoolean, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<bool, int>(length).AppendForReader(length, ref reader, ReadBoolean, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<bool, long>(length).AppendForReader(length, ref reader, ReadBoolean, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<bool, float>(length).AppendForReader(length, ref reader, ReadBoolean, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<bool, double>(length).AppendForReader(length, ref reader, ReadBoolean, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Uint8 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<byte, string>(length).AppendForReader(length, ref reader, ReadUint8, ReadString),
                    FieldValueType.Boolean => new Dictionary<byte, bool>(length).AppendForReader(length, ref reader, ReadUint8, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<byte, byte>(length).AppendForReader(length, ref reader, ReadUint8, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<byte, ushort>(length).AppendForReader(length, ref reader, ReadUint8, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<byte, uint>(length).AppendForReader(length, ref reader, ReadUint8, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<byte, ulong>(length).AppendForReader(length, ref reader, ReadUint8, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<byte, sbyte>(length).AppendForReader(length, ref reader, ReadUint8, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<byte, short>(length).AppendForReader(length, ref reader, ReadUint8, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<byte, int>(length).AppendForReader(length, ref reader, ReadUint8, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<byte, long>(length).AppendForReader(length, ref reader, ReadUint8, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<byte, float>(length).AppendForReader(length, ref reader, ReadUint8, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<byte, double>(length).AppendForReader(length, ref reader, ReadUint8, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Uint16 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<ushort, string>(length).AppendForReader(length, ref reader, ReadUint16, ReadString),
                    FieldValueType.Boolean => new Dictionary<ushort, bool>(length).AppendForReader(length, ref reader, ReadUint16, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<ushort, byte>(length).AppendForReader(length, ref reader, ReadUint16, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<ushort, ushort>(length).AppendForReader(length, ref reader, ReadUint16, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<ushort, uint>(length).AppendForReader(length, ref reader, ReadUint16, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<ushort, ulong>(length).AppendForReader(length, ref reader, ReadUint16, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<ushort, sbyte>(length).AppendForReader(length, ref reader, ReadUint16, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<ushort, short>(length).AppendForReader(length, ref reader, ReadUint16, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<ushort, int>(length).AppendForReader(length, ref reader, ReadUint16, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<ushort, long>(length).AppendForReader(length, ref reader, ReadUint16, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<ushort, float>(length).AppendForReader(length, ref reader, ReadUint16, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<ushort, double>(length).AppendForReader(length, ref reader, ReadUint16, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Uint32 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<uint, string>(length).AppendForReader(length, ref reader, ReadUint32, ReadString),
                    FieldValueType.Boolean => new Dictionary<uint, bool>(length).AppendForReader(length, ref reader, ReadUint32, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<uint, byte>(length).AppendForReader(length, ref reader, ReadUint32, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<uint, ushort>(length).AppendForReader(length, ref reader, ReadUint32, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<uint, uint>(length).AppendForReader(length, ref reader, ReadUint32, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<uint, ulong>(length).AppendForReader(length, ref reader, ReadUint32, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<uint, sbyte>(length).AppendForReader(length, ref reader, ReadUint32, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<uint, short>(length).AppendForReader(length, ref reader, ReadUint32, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<uint, int>(length).AppendForReader(length, ref reader, ReadUint32, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<uint, long>(length).AppendForReader(length, ref reader, ReadUint32, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<uint, float>(length).AppendForReader(length, ref reader, ReadUint32, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<uint, double>(length).AppendForReader(length, ref reader, ReadUint32, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Uint64 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<ulong, string>(length).AppendForReader(length, ref reader, ReadUint64, ReadString),
                    FieldValueType.Boolean => new Dictionary<ulong, bool>(length).AppendForReader(length, ref reader, ReadUint64, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<ulong, byte>(length).AppendForReader(length, ref reader, ReadUint64, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<ulong, ushort>(length).AppendForReader(length, ref reader, ReadUint64, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<ulong, uint>(length).AppendForReader(length, ref reader, ReadUint64, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<ulong, ulong>(length).AppendForReader(length, ref reader, ReadUint64, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<ulong, sbyte>(length).AppendForReader(length, ref reader, ReadUint64, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<ulong, short>(length).AppendForReader(length, ref reader, ReadUint64, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<ulong, int>(length).AppendForReader(length, ref reader, ReadUint64, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<ulong, long>(length).AppendForReader(length, ref reader, ReadUint64, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<ulong, float>(length).AppendForReader(length, ref reader, ReadUint64, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<ulong, double>(length).AppendForReader(length, ref reader, ReadUint64, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Int8 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<sbyte, string>(length).AppendForReader(length, ref reader, ReadInt8, ReadString),
                    FieldValueType.Boolean => new Dictionary<sbyte, bool>(length).AppendForReader(length, ref reader, ReadInt8, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<sbyte, byte>(length).AppendForReader(length, ref reader, ReadInt8, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<sbyte, ushort>(length).AppendForReader(length, ref reader, ReadInt8, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<sbyte, uint>(length).AppendForReader(length, ref reader, ReadInt8, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<sbyte, ulong>(length).AppendForReader(length, ref reader, ReadInt8, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<sbyte, sbyte>(length).AppendForReader(length, ref reader, ReadInt8, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<sbyte, short>(length).AppendForReader(length, ref reader, ReadInt8, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<sbyte, int>(length).AppendForReader(length, ref reader, ReadInt8, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<sbyte, long>(length).AppendForReader(length, ref reader, ReadInt8, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<sbyte, float>(length).AppendForReader(length, ref reader, ReadInt8, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<sbyte, double>(length).AppendForReader(length, ref reader, ReadInt8, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Int16 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<short, string>(length).AppendForReader(length, ref reader, ReadInt16, ReadString),
                    FieldValueType.Boolean => new Dictionary<short, bool>(length).AppendForReader(length, ref reader, ReadInt16, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<short, byte>(length).AppendForReader(length, ref reader, ReadInt16, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<short, ushort>(length).AppendForReader(length, ref reader, ReadInt16, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<short, uint>(length).AppendForReader(length, ref reader, ReadInt16, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<short, ulong>(length).AppendForReader(length, ref reader, ReadInt16, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<short, sbyte>(length).AppendForReader(length, ref reader, ReadInt16, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<short, short>(length).AppendForReader(length, ref reader, ReadInt16, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<short, int>(length).AppendForReader(length, ref reader, ReadInt16, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<short, long>(length).AppendForReader(length, ref reader, ReadInt16, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<short, float>(length).AppendForReader(length, ref reader, ReadInt16, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<short, double>(length).AppendForReader(length, ref reader, ReadInt16, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Int32 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<int, string>(length).AppendForReader(length, ref reader, ReadInt32, ReadString),
                    FieldValueType.Boolean => new Dictionary<int, bool>(length).AppendForReader(length, ref reader, ReadInt32, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<int, byte>(length).AppendForReader(length, ref reader, ReadInt32, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<int, ushort>(length).AppendForReader(length, ref reader, ReadInt32, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<int, uint>(length).AppendForReader(length, ref reader, ReadInt32, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<int, ulong>(length).AppendForReader(length, ref reader, ReadInt32, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<int, sbyte>(length).AppendForReader(length, ref reader, ReadInt32, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<int, short>(length).AppendForReader(length, ref reader, ReadInt32, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<int, int>(length).AppendForReader(length, ref reader, ReadInt32, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<int, long>(length).AppendForReader(length, ref reader, ReadInt32, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<int, float>(length).AppendForReader(length, ref reader, ReadInt32, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<int, double>(length).AppendForReader(length, ref reader, ReadInt32, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Int64 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<long, string>(length).AppendForReader(length, ref reader, ReadInt64, ReadString),
                    FieldValueType.Boolean => new Dictionary<long, bool>(length).AppendForReader(length, ref reader, ReadInt64, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<long, byte>(length).AppendForReader(length, ref reader, ReadInt64, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<long, ushort>(length).AppendForReader(length, ref reader, ReadInt64, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<long, uint>(length).AppendForReader(length, ref reader, ReadInt64, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<long, ulong>(length).AppendForReader(length, ref reader, ReadInt64, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<long, sbyte>(length).AppendForReader(length, ref reader, ReadInt64, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<long, short>(length).AppendForReader(length, ref reader, ReadInt64, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<long, int>(length).AppendForReader(length, ref reader, ReadInt64, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<long, long>(length).AppendForReader(length, ref reader, ReadInt64, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<long, float>(length).AppendForReader(length, ref reader, ReadInt64, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<long, double>(length).AppendForReader(length, ref reader, ReadInt64, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Float32 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<float, string>(length).AppendForReader(length, ref reader, ReadFloat32, ReadString),
                    FieldValueType.Boolean => new Dictionary<float, bool>(length).AppendForReader(length, ref reader, ReadFloat32, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<float, byte>(length).AppendForReader(length, ref reader, ReadFloat32, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<float, ushort>(length).AppendForReader(length, ref reader, ReadFloat32, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<float, uint>(length).AppendForReader(length, ref reader, ReadFloat32, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<float, ulong>(length).AppendForReader(length, ref reader, ReadFloat32, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<float, sbyte>(length).AppendForReader(length, ref reader, ReadFloat32, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<float, short>(length).AppendForReader(length, ref reader, ReadFloat32, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<float, int>(length).AppendForReader(length, ref reader, ReadFloat32, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<float, long>(length).AppendForReader(length, ref reader, ReadFloat32, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<float, float>(length).AppendForReader(length, ref reader, ReadFloat32, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<float, double>(length).AppendForReader(length, ref reader, ReadFloat32, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                FieldValueType.Float64 => valueValueType switch
                {
                    FieldValueType.String => new Dictionary<double, string>(length).AppendForReader(length, ref reader, ReadFloat64, ReadString),
                    FieldValueType.Boolean => new Dictionary<double, bool>(length).AppendForReader(length, ref reader, ReadFloat64, ReadBoolean),
                    FieldValueType.Uint8 => new Dictionary<double, byte>(length).AppendForReader(length, ref reader, ReadFloat64, ReadUint8),
                    FieldValueType.Uint16 => new Dictionary<double, ushort>(length).AppendForReader(length, ref reader, ReadFloat64, ReadUint16),
                    FieldValueType.Uint32 => new Dictionary<double, uint>(length).AppendForReader(length, ref reader, ReadFloat64, ReadUint32),
                    FieldValueType.Uint64 => new Dictionary<double, ulong>(length).AppendForReader(length, ref reader, ReadFloat64, ReadUint64),
                    FieldValueType.Int8 => new Dictionary<double, sbyte>(length).AppendForReader(length, ref reader, ReadFloat64, ReadInt8),
                    FieldValueType.Int16 => new Dictionary<double, short>(length).AppendForReader(length, ref reader, ReadFloat64, ReadInt16),
                    FieldValueType.Int32 => new Dictionary<double, int>(length).AppendForReader(length, ref reader, ReadFloat64, ReadInt32),
                    FieldValueType.Int64 => new Dictionary<double, long>(length).AppendForReader(length, ref reader, ReadFloat64, ReadInt64),
                    FieldValueType.Float32 => new Dictionary<double, float>(length).AppendForReader(length, ref reader, ReadFloat64, ReadFloat32),
                    FieldValueType.Float64 => new Dictionary<double, double>(length).AppendForReader(length, ref reader, ReadFloat64, ReadFloat64),
                    FieldValueType.Custom => throw new NotImplementedException(),
                    _ => throw new ArgumentException($"Map cannot have values of type {valueValueType}")
                },
                _ => throw new ArgumentException($"Map cannot have keys of type {keyValueType}"),
            };
        }

        private static object ReadList(ref MessagePackReader reader, FieldValueType typeArgument)
        {
            int length = reader.ReadArrayHeader();

            return typeArgument switch
            {
                FieldValueType.String => new List<string>(length).AppendForReader(ref reader, ReadString),
                FieldValueType.Boolean => new List<bool>(length).AppendForReader(ref reader, ReadBoolean),
                FieldValueType.Uint8 => new List<byte>(length).AppendForReader(ref reader, ReadUint8),
                FieldValueType.Uint16 => new List<ushort>(length).AppendForReader(ref reader, ReadUint16),
                FieldValueType.Uint32 => new List<uint>(length).AppendForReader(ref reader, ReadUint32),
                FieldValueType.Uint64 => new List<ulong>(length).AppendForReader(ref reader, ReadUint64),
                FieldValueType.Int8 => new List<sbyte>(length).AppendForReader(ref reader, ReadInt8),
                FieldValueType.Int16 => new List<short>(length).AppendForReader(ref reader, ReadInt16),
                FieldValueType.Int32 => new List<int>(length).AppendForReader(ref reader, ReadInt32),
                FieldValueType.Int64 => new List<long>(length).AppendForReader(ref reader, ReadInt64),
                FieldValueType.Float32 => new List<float>(length).AppendForReader(ref reader, ReadFloat32),
                FieldValueType.Float64 => new List<double>(length).AppendForReader(ref reader, ReadFloat64),
                FieldValueType.Custom => throw new NotImplementedException(),
                _ => throw new ArgumentException($"List cannot have arguments of type {typeArgument}"),
            };
        }

        private static string ReadString(ref MessagePackReader reader) => reader.ReadString();
        private static bool ReadBoolean(ref MessagePackReader reader) => reader.ReadBoolean();
        private static byte ReadUint8(ref MessagePackReader reader) => reader.ReadByte();
        private static ushort ReadUint16(ref MessagePackReader reader) => reader.ReadUInt16();
        private static uint ReadUint32(ref MessagePackReader reader) => reader.ReadUInt32();
        private static ulong ReadUint64(ref MessagePackReader reader) => reader.ReadUInt64();
        private static sbyte ReadInt8(ref MessagePackReader reader) => reader.ReadSByte();
        private static short ReadInt16(ref MessagePackReader reader) => reader.ReadInt16();
        private static int ReadInt32(ref MessagePackReader reader) => reader.ReadInt32();
        private static long ReadInt64(ref MessagePackReader reader) => reader.ReadInt64();
        private static float ReadFloat32(ref MessagePackReader reader) => reader.ReadSingle();
        private static double ReadFloat64(ref MessagePackReader reader) => reader.ReadDouble(); 
       
        #endregion

        private static void VerifyFieldType(FieldInfo field, Type valueType)
        {
            var fieldType = field.ValueType;
            TypeCode typeCode;
            if(field.Nullable)
            {
                valueType = Nullable.GetUnderlyingType(valueType)!;
                typeCode = Type.GetTypeCode(valueType);
            }
            else
                typeCode = Type.GetTypeCode(valueType);

            bool valid = InternalVerifyFieldType(typeCode, fieldType, field.CustomTypeInfo, valueType);
            if (!valid)
                throw new InvalidFieldTypeError(field.Index, valueType, fieldType);
        }

        private static void VerifyListType(FieldInfo field, Type typeArgument)
        {
            var typeArgumentTypeCode = Type.GetTypeCode(typeArgument);

            if (field.ValueType != FieldValueType.List)
                throw new FieldNotAListException(field.Index);

            var genericArgumentValueType = field.TypeArguments![0].ValueType;
            bool valid = InternalVerifyFieldType(typeArgumentTypeCode, genericArgumentValueType, null, null);
            if (!valid)
                throw new InvalidFieldTypeError(field.Index, typeArgument, genericArgumentValueType);
        }

        private static void VerifyMapType(FieldInfo field, Type keyArgument, Type valueArgument)
        {
            var keyArgumentTypeCode = Type.GetTypeCode(keyArgument);
            var valueArgumentTypeCode = Type.GetTypeCode(valueArgument);

            if (field.ValueType != FieldValueType.Map)
                throw new FieldNotAMapException(field.Index);

            var genericArgumentKeyValueType = field.TypeArguments![0].ValueType;
            var genericArgumentValueValueType = field.TypeArguments![1].ValueType;
            bool valid = InternalVerifyFieldType(keyArgumentTypeCode, genericArgumentKeyValueType, null, null);
            if (!valid)
                throw new InvalidFieldTypeError(field.Index, keyArgument, genericArgumentKeyValueType);

            valid = InternalVerifyFieldType(valueArgumentTypeCode, genericArgumentValueValueType, null, null);
            if (!valid)
                throw new InvalidFieldTypeError(field.Index, valueArgument, genericArgumentValueValueType);
        }

        private static bool InternalVerifyFieldType(TypeCode typeCode, FieldValueType fieldType, Type? customType, Type? valueType)
        {
            return typeCode switch
            {
                TypeCode.Boolean => fieldType == FieldValueType.Boolean,
                TypeCode.String => fieldType == FieldValueType.String,
                TypeCode.SByte => fieldType == FieldValueType.Int8,
                TypeCode.Int16 => fieldType == FieldValueType.Int16,
                TypeCode.Int32 => fieldType == FieldValueType.Int32,
                TypeCode.Int64 => fieldType == FieldValueType.Int64,
                TypeCode.Byte => fieldType == FieldValueType.Uint8,
                TypeCode.UInt16 => fieldType == FieldValueType.Uint16,
                TypeCode.UInt32 => fieldType == FieldValueType.Uint32,
                TypeCode.UInt64 => fieldType == FieldValueType.Uint64,
                TypeCode.Double => fieldType == FieldValueType.Float64,
                TypeCode.Single => fieldType == FieldValueType.Float32,
                TypeCode.Object => customType == valueType,
                _ => false,
            };
        }

        #endregion
    }
}