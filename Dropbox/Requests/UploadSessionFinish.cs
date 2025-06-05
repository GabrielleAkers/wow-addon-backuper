using System.Text.Json.Serialization;

namespace Dropbox.Requests;

public record class UploadSessionFinish : BaseRequest
{
    [JsonPropertyName("commit")]
    public required UploadSessionCommit Commit { get; set; }

    [JsonPropertyName("cursor")]
    public required UploadSessionCursor Cursor { get; set; }
}