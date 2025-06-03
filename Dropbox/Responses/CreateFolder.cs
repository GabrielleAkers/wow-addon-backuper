using System.Text.Json.Serialization;

namespace Dropbox.Responses;

public class CreateFolderMetadata
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("path_display")]
    public string? PathDisplay { get; set; }

    [JsonPropertyName("path_lower")]
    public string? PathLower { get; set; }
}

public class CreateFolder
{
    [JsonPropertyName("metadata")]
    public CreateFolderMetadata? Metadata { get; set; }
}