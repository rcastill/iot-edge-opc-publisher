namespace OpcPublisher
{
    using System;
    using Newtonsoft.Json;
    using System.Linq;
    using Opc.Ua;

    public class WriteValuePayload
    {
        [JsonProperty("EndpointUrl"), JsonRequired]
        public Uri EndpointUri { get; set; }
        [JsonRequired]
        public string ExtendedNodeId { get; set; }
        // If type is not specified, it tries to guess it
        public string Type { get; set; }
        [JsonRequired]
        public dynamic Value { get; set; }

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                // tries to guess
                if (string.IsNullOrEmpty(Type))
                {
                    return true;
                }
                var supportedTypes = new string[] {
                    "byte",
                    "string",
                    "double",
                    "float",
                    "ulong",
                    "long",
                    "uint",
                    "int",
                    "ushort",
                    "short",
                    "sbyte",
                    "bool"
                };
                return supportedTypes
                    // works with arays too
                    .Contains(Type.Replace("[]", ""));
            }
        }

        [JsonIgnore]
        public Variant Variant
        {
            get
            {
                if (!IsValid)
                {
                    return Variant.Null;
                }
                if (string.IsNullOrEmpty(Type))
                {
                    return new Variant(Value);
                }
                switch (Type)
                {
                    case "byte":
                        return (byte)Value;
                    case "byte[]":
                        return Value.ToObject<byte[]>();
                    case "string":
                        return (string)Value;
                    case "string[]":
                        return Value.ToObject<string[]>();
                    case "double":
                        return (double)Value;
                    case "double[]":
                        return Value.ToObject<double[]>();
                    case "float":
                        return (float)Value;
                    case "float[]":
                        return Value.ToObject<float[]>();
                    case "ulong":
                        return (ulong)Value;
                    case "ulong[]":
                        return Value.ToObject<ulong[]>();
                    case "long":
                        return (long)Value;
                    case "long[]":
                        return Value.ToObject<long[]>();
                    case "uint":
                        return (uint)Value;
                    case "uint[]":
                        return Value.ToObject<uint[]>();
                    case "int":
                        return (int)Value;
                    case "int[]":
                        return Value.ToObject<int[]>();
                    case "ushort":
                        return (ushort)Value;
                    case "ushort[]":
                        return Value.ToObject<ushort[]>();
                    case "short":
                        return (short)Value;
                    case "short[]":
                        return Value.ToObject<short[]>();
                    case "sbyte":
                        return (sbyte)Value;
                    case "sbyte[]":
                        return Value.ToObject<sbyte[]>();
                    case "bool":
                        return (bool)Value;
                    case "bool[]":
                        return Value.ToObject<bool[]>();
                    default:
                        throw new InvalidOperationException("Unreachable code: WriteValuePayload.Variant");
                }
            }
        }
    }
}