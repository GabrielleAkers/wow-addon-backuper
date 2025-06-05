using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dropbox.Responses;

public record class BatchSessionStart
{
    [JsonPropertyName("session_ids")]
    public List<string> SessionIds { get; set; } = [];
}