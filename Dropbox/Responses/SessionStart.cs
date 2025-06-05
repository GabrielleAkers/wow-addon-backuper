using System.Text.Json.Serialization;

namespace Dropbox.Responses;

public record class SessionStart
{
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }
}