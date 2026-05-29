using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tibar.API.Converters;

public class EmptyStringDateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            throw new JsonException("O campo de data não pode estar vazio.");

        if (!DateOnly.TryParse(value, out var date))
            throw new JsonException($"Formato de data inválido: '{value}'. Espera-se uma data válida (yyyy-MM-dd).");

        return date;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O"));
    }
}
