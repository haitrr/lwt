namespace Lwt.Models
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// language code json converter.
    /// </summary>
    public class LearningLevelJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LearningLevel);
        }

        /// <inheritdoc />
        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException();
            }

            var code = serializer.Deserialize<string>(reader)!;
            return LearningLevel.GetFromString(code);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) {
                throw new Exception("unexpected value");
            }
            var item = (LearningLevel)value;
            writer.WriteValue(item.ToString());
            writer.Flush();
        }
    }
}