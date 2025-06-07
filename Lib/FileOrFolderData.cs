namespace wow_addon_backuper;

public record class FileOrFolderData
{
    public required string? Name { get; set; }
    public required string? Path { get; set; }
    public required string? RemotePath { get; set; }
}