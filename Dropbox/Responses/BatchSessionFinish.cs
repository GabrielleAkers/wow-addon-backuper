using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dropbox.Responses;

public record class LookupFailedFailure
{
    [JsonPropertyName(".tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("correct_offset")]
    public long CorrectOffset { get; set; }
}

public record class BatchSessionFinishFailure
{
    [JsonPropertyName(".tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("lookup_failed")]
    public LookupFailedFailure? LookupFailed { get; set; }
}

public record class BatchSessionFinishEntry : FileMetadata
{
    [JsonPropertyName(".tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("failure")]
    public BatchSessionFinishFailure? Failure { get; set; }
}

public record class BatchSessionFinish
{
    [JsonPropertyName("entries")]
    public List<BatchSessionFinishEntry> Entries { get; set; } = [];
}