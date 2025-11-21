using System.Text.Json.Serialization;

namespace WebApplication1.Utils.Exceptions;

public record ExceptionMessage
{
    public string Key { get; init; } = string.Empty;
    public string SubKey { get; init; } = string.Empty;
    public Message Message { get; init; } = new();
}

public record Message
{
    [JsonPropertyName("sr-Latn")]
    public string Sr { get; init; } = string.Empty;

    [JsonPropertyName("en-US")]
    public string Eng { get; init; } = string.Empty;
}
