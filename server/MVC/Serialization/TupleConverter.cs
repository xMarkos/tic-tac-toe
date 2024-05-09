using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Markos.TicTacToe.MVC.Serialization;

/// <summary>
/// Creates converter for serializing C# value tuples into fixed length JSON arrays.
/// </summary>
internal class TupleConverterFactory : JsonConverterFactory
{
    private readonly static TupleConverter _instance = new();

    public override bool CanConvert(Type typeToConvert)
        => typeof(ITuple).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => _instance;

    private class TupleConverter : JsonConverter<ITuple>
    {
        public override ITuple? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ITuple value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();

            for (int i = 0; i < value.Length; i++)
            {
                JsonSerializer.Serialize(writer, value[i], options);
            }

            writer.WriteEndArray();
        }
    }
}
