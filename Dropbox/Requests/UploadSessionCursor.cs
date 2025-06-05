using System.Text.Json.Serialization;

namespace Dropbox.Requests;

public record class UploadSessionCursor
{
    [JsonPropertyName("offset")]
    public long Offset { get; set; }

    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }
}