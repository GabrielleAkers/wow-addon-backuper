using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dropbox.Requests;

public record class UploadSessionAppendBatchEntry : UploadSessionAppend
{
    [JsonPropertyName("length")]
    public required long Length { get; set; }
}

public record class UploadSessionAppendBatch : BaseRequest
{
    [JsonPropertyName("entries")]
    public required List<UploadSessionAppendBatchEntry> Entries { get; set; } = [];
}