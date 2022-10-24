using MessagePackSchema.Runtime.Serializers;

namespace MessagePackSchema.Runtime.Impl
{
    internal static class TypeSerializerPool
    {
        public static readonly IDictionary<Type, Lazy<ITypeSerializer>> TypeSerializers = new Dictionary<Type, Lazy<ITypeSerializer>>()
        {
            { typeof(DateTime), new Lazy<ITypeSerializer>(() => new DateTimeTypeSerializer()) },
            { typeof(DateTimeOffset), new Lazy<ITypeSerializer>(() => new DateTimeOffsetTypeSerializer()) },
            { typeof(DateTime?), new Lazy<ITypeSerializer>(() => new NullableTypeSerializer<DateTime>()) },
            { typeof(DateTimeOffset?), new Lazy<ITypeSerializer>(() => new NullableTypeSerializer<DateTimeOffset>()) },
            { typeof(TimeSpan), new Lazy<ITypeSerializer>(() => new TimeSpanTypeSerializer()) },
            { typeof(TimeSpan?), new Lazy<ITypeSerializer>(() => new NullableTypeSerializer<TimeSpan>()) },
        };

        public static void CreateAndRegister<TSerializer, TType>() where TSerializer : ITypeSerializer<TType>
        {
            var serializer = new Lazy<ITypeSerializer>(() => Activator.CreateInstance<TSerializer>(), true);
            TypeSerializers[typeof(TType)] = serializer;
        }
    }
}