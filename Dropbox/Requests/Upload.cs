using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class Upload : BaseRequest
{
    [JsonPropertyName("autorename")]
    public bool Autorename { get; set; } = false;

    [JsonPropertyName("mode")]
    public string? Mode { get; set; } = "overwrite";

    [JsonPropertyName("mute")]
    public bool Mute { get; set; } = false;

    [JsonPropertyName("strict_conflict")]
    public bool StrictConflict { get; set; } = false;

    [JsonPropertyName("path")]
    public required string Path { get; set; }
}