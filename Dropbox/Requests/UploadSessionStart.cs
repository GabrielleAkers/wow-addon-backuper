using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class UploadSessionStart : BaseRequest
{
    [JsonPropertyName("close")]
    public bool Close { get; set; } = false;
}