using Avalonia.Platform.Storage;

namespace wow_addon_backuper;

public enum AddonsOrAccount
{
    AddOns,
    [StringValue("Account Settings")]
    Account
}

public record class AddonsOrAccountFolderDataRow(IStorageItem StorageItem)
{
    public required bool IsSelected { get; set; }
}