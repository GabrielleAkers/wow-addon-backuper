using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class UploadSessionStartBatch : BaseRequest
{
    [JsonPropertyName("num_sessions")]
    public required int NumSessions { get; set; }
}