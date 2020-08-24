namespace Lwt.Models
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// language code json converter.
    /// </summary>
    public class LanguageCodeJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LanguageCode) || objectType == typeof(string);
        }

        /// <inheritdoc />
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException();
            }

            var code = serializer.Deserialize<string>(reader);
            return LanguageCode.GetFromString(code);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (LanguageCode)value;
            writer.WriteValue(item.ToString());
            writer.Flush();
        }
    }
}