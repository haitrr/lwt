namespace Lwt.Models
{
    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [JsonConverter(typeof(LanguageCodeJsonConverter))]
    public sealed class LanguageCode : ISerializable
    {
        private readonly string value;

        public static readonly LanguageCode ENGLISH = new LanguageCode("en");
        public static readonly LanguageCode VIETNAMESE = new LanguageCode("vi");
        public static readonly LanguageCode CHINESE = new LanguageCode("zh");
        public static readonly LanguageCode JAPANESE = new LanguageCode("ja");

        private LanguageCode(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return this.value;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("value", this.value);
        }
    }

    public class LanguageCodeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LanguageCode);
        }

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

            switch (code)
            {
                case "en":
                    return LanguageCode.ENGLISH;
                case "zh":
                    return LanguageCode.CHINESE;
                case "vi":
                    return LanguageCode.VIETNAMESE;
                case "ja":
                    return LanguageCode.JAPANESE;
            }

            return new SerializationException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (LanguageCode)value;
            writer.WriteValue(item.ToString());
            writer.Flush();
        }
    }
}