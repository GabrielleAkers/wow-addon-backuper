using System.Text.Json.Serialization;

namespace Dropbox.Requests;

record class ListFolderContinue : BaseRequest
{
    [JsonPropertyName("cursor")]
    public required string? Cursor { get; set; }
}