using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class CheckUser : BaseRequest
{
    [JsonPropertyName("query")]
    public required string? Query { get; set; }
}