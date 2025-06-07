using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class ListFolder : BaseRequest
{
    [JsonPropertyName("path")]
    public required string? Path { get; set; }

    [JsonPropertyName("include_mounted_folders")]
    public bool IncludeMountedFolders { get; set; } = false;

    [JsonPropertyName("include_non_downloadable_files")]
    public bool IncludeNonDownloadableFiles { get; set; } = false;

    [JsonPropertyName("recursive")]
    public bool Recursive { get; set; } = false;

    [JsonPropertyName("limit")]
    public int Limit { get; set; } = 2000;

    [JsonPropertyName("include_media_info")]
    public bool IncludeMediaInfo { get; set; } = false;

    [JsonPropertyName("include_has_explicit_shared_members")]
    public bool IncludeHasExplicitSharedMembers { get; set; } = false;

    [JsonPropertyName("include_deleted")]
    public bool IncludeDeleted { get; set; } = false;
}