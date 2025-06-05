using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dropbox.Requests;

public record class UploadSessionFinishBatch : BaseRequest
{
    [JsonPropertyName("entries")]
    public required List<UploadSessionFinish> Entries { get; set; } = [];
}