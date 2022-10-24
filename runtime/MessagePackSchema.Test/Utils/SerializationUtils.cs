using MessagePack;
using System.Buffers;

namespace MessagePackSchema.Test.Utils
{
    public static class SerializationUtils
    {
        public static byte[] SerializeDebug(this DefaultType defaultType)
        {
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(buffer);

            writer.Write(defaultType.Name);

            writer.WriteArrayHeader(defaultType.Names.Count);
            foreach (var name in defaultType.Names)
                writer.Write(name);

            writer.WriteMapHeader(defaultType.Config.Count);
            foreach(var entry in defaultType.Config)
            {
                writer.Write(entry.Key);
                writer.Write(entry.Value);
            }

            if (defaultType.MyUint16 == null) writer.WriteNil(); else writer.WriteUInt16((ushort)defaultType.MyUint16);

            writer.Flush();
            return buffer.WrittenMemory.ToArray();
        }
    }
}