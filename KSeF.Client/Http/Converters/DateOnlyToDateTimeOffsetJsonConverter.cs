using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KSeF.Client.Http.Converters
{
    /// <summary>
    /// Konwerter JSON, który akceptuje wartości zawierające wyłącznie daty (ISO yyyy-MM-dd)
    /// oraz inne wartości daty/czasu i mapuje je na <see cref="DateTimeOffset"/>.
    /// Podczas serializacji zapisuje wyłącznie datę (yyyy-MM-dd).
    /// </summary>
    public sealed class DateOnlyToDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    {
        private const string DateOnlyFormat = "yyyy-MM-dd";

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string text = reader.GetString() ?? string.Empty;

                if (DateTime.TryParseExact(text, DateOnlyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateOnly))
                {
                    return new DateTimeOffset(DateTime.SpecifyKind(dateOnly.Date, DateTimeKind.Utc));
                }

                if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset dateTimeOffSet))
                {
                    return dateTimeOffSet;
                }

                throw new JsonException($"Unable to parse '{text}' to {nameof(DateTimeOffset)}.");
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out long epoch))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(epoch);
                }
            }

            throw new JsonException($"Unexpected token parsing {nameof(DateTimeOffset)}. Token: {reader.TokenType}.");
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            DateTime utcDate = value.UtcDateTime.Date;
            string formatted = utcDate.ToString(DateOnlyFormat, CultureInfo.InvariantCulture);
            writer.WriteStringValue(formatted);
        }
    }
}
