using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class Download : BaseRequest
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }
}