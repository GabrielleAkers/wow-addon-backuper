using System.Text.Json.Serialization;

namespace Dropbox.Responses;

public record class FileMetadata
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("path_display")]
    public string? PathDisplay { get; set; }

    [JsonPropertyName("path_lower")]
    public string? PathLower { get; set; }

    [JsonPropertyName("content_hash")]
    public string? ContentHash { get; set; }
}