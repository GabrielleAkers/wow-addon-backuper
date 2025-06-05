using System.Text.Json.Serialization;

namespace Dropbox.Requests;

public record class UploadSessionCommit
{
    [JsonPropertyName("autorename")]
    public bool Autorename { get; set; } = false;

    [JsonPropertyName("mode")]
    public string? Mode { get; set; } = "overwrite";

    [JsonPropertyName("mute")]
    public bool Mute { get; set; } = false;

    [JsonPropertyName("path")]
    public required string Path { get; set; }

    [JsonPropertyName("strict_conflict")]
    public bool StrictConflict { get; set; } = false;
}
