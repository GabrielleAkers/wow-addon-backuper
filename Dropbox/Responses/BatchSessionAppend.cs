using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dropbox.Responses;

public record class BatchSessionAppendFailure
{
    [JsonPropertyName(".tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("correct_offset")]
    public long? CorrectOffset { get; set; }
}

public record class BatchSessionEntry
{
    [JsonPropertyName(".tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("failure")]
    public BatchSessionAppendFailure? Failure { get; set; }
}

public record class BatchSessionAppend
{
    [JsonPropertyName("entries")]
    public List<BatchSessionEntry> Entries { get; set; } = [];
}