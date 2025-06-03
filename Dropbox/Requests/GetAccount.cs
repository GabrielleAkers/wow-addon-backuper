using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class GetAccount : BaseRequest
{
    [JsonPropertyName("account_id")]
    public required string? AccountId { get; set; }
}