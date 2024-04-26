using System.Text.Json;
using System.Text.Json.Serialization;

namespace Markos.TicTacToe.MVC.Serialization;

/// <summary>
/// Creates converters for serializing 2D arrays to JSON.
/// </summary>
internal class TwoDArrayConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsArray && typeToConvert.GetArrayRank() == 2;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type itemType = typeToConvert.GetElementType()!;
        return (JsonConverter?)Activator.CreateInstance(typeof(TwoDArrayConverter<>).MakeGenericType(itemType));
    }
}
