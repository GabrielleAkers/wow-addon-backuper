using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dropbox.Responses;

public record class FolderEntry
{
    [JsonPropertyName("client_modified")]
    public string? client_modified { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("path_display")]
    public string? PathDisplay { get; set; }

    [JsonPropertyName("path_lower")]
    public string? PathLower { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }
}

public record class ListFolder
{
    [JsonPropertyName("cursor")]
    public string? Cursor { get; set; }

    [JsonPropertyName("entries")]
    public List<FolderEntry>? Entries { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("error_summary")]
    public string? ErrorSummary { get; set; }
}