using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class CreateFolder : BaseRequest
{
    [JsonPropertyName("path")]
    public required string? Path { get; set; }

    [JsonPropertyName("autorename")]
    public bool Autorename { get; set; } = false;
}