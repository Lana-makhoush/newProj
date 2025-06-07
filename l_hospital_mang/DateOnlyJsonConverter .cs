namespace l_hospital_mang
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class DateOnlyJsonConverter : JsonConverter<DateTime?>
    {
        private readonly string format = "yyyy-MM-dd";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (DateTime.TryParse(value, out var date))
                return date;
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(format));
            else
                writer.WriteNullValue();
        }
    }

}
