using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace K2.Common.JsonConverter
{
    public class AbstractClassConverterWithTypeDiscriminator<T> : JsonConverter<T> where T : class
    {
        private const string TYPE_DISCRIMINATOR = "$typeDiscriminator";
        private const string WRAPPED_VALUE = "$wrappedValue";
        private const string WRAPPED_VALUES = "$wrappedValues";

        private readonly Lazy<Dictionary<string, Type>> _types = new(() =>
        {
            var sourceType = typeof(T);
            var resTypeToWatch = sourceType.HasElementType ? sourceType.GetElementType() : sourceType;
            return Assembly.GetAssembly(resTypeToWatch)
                .GetTypes()
                .Where(x => x.IsClass && (x == resTypeToWatch || x.IsSubclassOf(resTypeToWatch)))
                .ToDictionary(k => k.Name, v => v);
        });

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            object ExtractObject(ref Utf8JsonReader reader)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();
                if (propertyName != TYPE_DISCRIMINATOR)
                {
                    throw new JsonException();
                }

                reader.Read();
                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException();
                }
                var typeDiscriminator = reader.GetString();
                var typeValue = _types.Value[typeDiscriminator];

                reader.Read();
                var wrappedValue = reader.GetString();
                switch (wrappedValue)
                {
                    case WRAPPED_VALUES:
                        reader.Read();
                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new JsonException();

                        var elements = new List<object>();
                        reader.Read();
                        while (reader.TokenType != JsonTokenType.EndArray)
                        {
                            elements.Add(ExtractObject(ref reader));
                            reader.Read();
                        }
                        reader.Read();
                        var resultArray = Array.CreateInstance(typeValue, elements.Count);
                        for (var i = 0; i < elements.Count; i++)
                            resultArray.SetValue(elements[i], i);
                        return resultArray;
                    case WRAPPED_VALUE:
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException();
                        var extractedItem = JsonSerializer.Deserialize(ref reader, typeValue, options);
                        reader.Read();
                        return extractedItem;
                    default:
                        throw new JsonException();
                }
            }

            return ExtractObject(ref reader) as T;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            void AppendObject(object objectToAppend)
            {
                if (objectToAppend is Array asArray)
                {
                    writer.WriteStartObject();
                    writer.WriteString(TYPE_DISCRIMINATOR, objectToAppend.GetType().GetElementType().Name);
                    writer.WritePropertyName(WRAPPED_VALUES);
                    writer.WriteStartArray();
                    foreach (var arrayItem in asArray)
                    {
                        AppendObject(arrayItem);
                    }
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WriteString(TYPE_DISCRIMINATOR, objectToAppend.GetType().Name);
                    writer.WritePropertyName(WRAPPED_VALUE);
                    JsonSerializer.Serialize(writer, objectToAppend, objectToAppend.GetType(), options);
                    writer.WriteEndObject();
                }
            }

            AppendObject(value);
        }
    }
}