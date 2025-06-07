using Avalonia.Platform.Storage;

namespace wow_addon_backuper;

public enum AddonsOrAccount
{
    AddOns,
    [StringValue("Account Settings")]
    Account
}

public record class AddonsOrAccountFolderDataRow : FileOrFolderData
{
    public required bool IsSelected { get; set; }
    public bool IsLoading { get; set; } = false;
}