using System.Text.Json.Serialization;

namespace Dropbox.Requests;

public record class UploadSessionAppend : BaseRequest
{
    [JsonPropertyName("close")]
    public bool Close { get; set; } = false;


    [JsonPropertyName("cursor")]
    public required UploadSessionCursor Cursor { get; set; }
}