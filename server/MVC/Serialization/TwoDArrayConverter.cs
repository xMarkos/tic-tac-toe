using System.Text.Json;
using System.Text.Json.Serialization;

namespace Markos.TicTacToe.MVC.Serialization;

/// <summary>
/// Converts a 2D array to JSON.
/// </summary>
/// <remarks>
/// The JSON is represented as array of arrays, i.e. T[][].
/// </remarks>
internal class TwoDArrayConverter<T> : JsonConverter<T[,]>
{
    public override T[,]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, T[,] value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        for (int y = 0, l1 = value.GetLength(1); y < l1; y++)
        {
            writer.WriteStartArray();

            for (int x = 0, l0 = value.GetLength(0); x < l0; x++)
            {
                JsonSerializer.Serialize(writer, value[x, y], options);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }
}
