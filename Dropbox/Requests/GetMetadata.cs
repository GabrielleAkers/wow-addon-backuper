using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class GetMetadata : BaseRequest
{
    [JsonPropertyName("include_deleted")]
    public bool IncludeDeleted { get; set; } = false;

    [JsonPropertyName("include_has_explicit_shared_members")]
    public bool IncludeHasExplicitSharedMembers { get; set; } = false;

    [JsonPropertyName("include_media_info")]
    public bool IncludeMediaInfo { get; set; } = false;

    [JsonPropertyName("path")]
    public required string? Path { get; set; }
}
