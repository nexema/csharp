﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using MessagePackSchema.Generator;
//
//    var generateInput = GenerateInput.FromJson(jsonString);

namespace MessagePackSchema.Generator
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class GenerateInput
    {
        [JsonProperty("packages", Required = Required.Always)]
        public Package[] Packages { get; set; }

        [JsonProperty("root", Required = Required.Always)]
        public string Root { get; set; }
    }

    public partial class Package
    {
        [JsonProperty("files", Required = Required.Always)]
        public File[] Files { get; set; }

        [JsonProperty("importTypeIds", Required = Required.Always)]
        public object[] ImportTypeIds { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("path", Required = Required.Always)]
        public string Path { get; set; }

        [JsonProperty("subPackages", Required = Required.Always)]
        public Package[] SubPackages { get; set; }
    }

    public partial class File
    {
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("path", Required = Required.Always)]
        public string Path { get; set; }

        [JsonProperty("types", Required = Required.AllowNull)]
        public FileType[] Types { get; set; }

        [JsonProperty("version", Required = Required.Always)]
        public long Version { get; set; }
    }

    public partial class FileType
    {
        [JsonProperty("fields", Required = Required.Always)]
        public Field[] Fields { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("modifier", Required = Required.Always)]
        public string Modifier { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
    }

    public partial class Field
    {
        [JsonProperty("defaultValue")]
        public object DefaultValue { get; set; }

        [JsonProperty("index", Required = Required.Always)]
        public long Index { get; set; }

        [JsonProperty("metadata")]
        public object Metadata { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public TypeArgumentElement Type { get; set; }
    }

    public partial class TypeArgumentElement
    {
        [JsonProperty("nullable", Required = Required.Always)]
        public bool Nullable { get; set; }

        [JsonProperty("primitive", Required = Required.Always)]
        public Primitive Primitive { get; set; }

        [JsonProperty("typeArguments", Required = Required.AllowNull)]
        public TypeArgumentElement[] TypeArguments { get; set; }

        [JsonProperty("typeName", Required = Required.Always)]
        public string TypeName { get; set; }
    }

    public enum Primitive { Binary, Boolean, Float32, Float64, Int16, Int32, Int64, Int8, List, Map, String, Uint16, Uint32, Uint64, Uint8 };

    public partial class GenerateInput
    {
        public static GenerateInput FromJson(string json) => JsonConvert.DeserializeObject<GenerateInput>(json, MessagePackSchema.Generator.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this GenerateInput self) => JsonConvert.SerializeObject(self, MessagePackSchema.Generator.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                PrimitiveConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class PrimitiveConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Primitive) || t == typeof(Primitive?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "binary":
                    return Primitive.Binary;
                case "boolean":
                    return Primitive.Boolean;
                case "float32":
                    return Primitive.Float32;
                case "float64":
                    return Primitive.Float64;
                case "int16":
                    return Primitive.Int16;
                case "int32":
                    return Primitive.Int32;
                case "int64":
                    return Primitive.Int64;
                case "int8":
                    return Primitive.Int8;
                case "list":
                    return Primitive.List;
                case "map":
                    return Primitive.Map;
                case "string":
                    return Primitive.String;
                case "uint16":
                    return Primitive.Uint16;
                case "uint32":
                    return Primitive.Uint32;
                case "uint64":
                    return Primitive.Uint64;
                case "uint8":
                    return Primitive.Uint8;
            }
            throw new Exception("Cannot unmarshal type Primitive");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Primitive)untypedValue;
            switch (value)
            {
                case Primitive.Binary:
                    serializer.Serialize(writer, "binary");
                    return;
                case Primitive.Boolean:
                    serializer.Serialize(writer, "boolean");
                    return;
                case Primitive.Float32:
                    serializer.Serialize(writer, "float32");
                    return;
                case Primitive.Float64:
                    serializer.Serialize(writer, "float64");
                    return;
                case Primitive.Int16:
                    serializer.Serialize(writer, "int16");
                    return;
                case Primitive.Int32:
                    serializer.Serialize(writer, "int32");
                    return;
                case Primitive.Int64:
                    serializer.Serialize(writer, "int64");
                    return;
                case Primitive.Int8:
                    serializer.Serialize(writer, "int8");
                    return;
                case Primitive.List:
                    serializer.Serialize(writer, "list");
                    return;
                case Primitive.Map:
                    serializer.Serialize(writer, "map");
                    return;
                case Primitive.String:
                    serializer.Serialize(writer, "string");
                    return;
                case Primitive.Uint16:
                    serializer.Serialize(writer, "uint16");
                    return;
                case Primitive.Uint32:
                    serializer.Serialize(writer, "uint32");
                    return;
                case Primitive.Uint64:
                    serializer.Serialize(writer, "uint64");
                    return;
                case Primitive.Uint8:
                    serializer.Serialize(writer, "uint8");
                    return;
            }
            throw new Exception("Cannot marshal type Primitive");
        }

        public static readonly PrimitiveConverter Singleton = new PrimitiveConverter();
    }
}
