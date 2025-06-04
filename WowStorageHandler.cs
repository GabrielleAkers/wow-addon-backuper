using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace wow_addon_backuper;

class WowStorageHandler(IStorageProvider storageProvider, string installDir, List<WowVersions> installedVersions)
{
    IStorageProvider _storageProvider = storageProvider;
    string _installDir = installDir;
    List<WowVersions> _installedVersions = installedVersions;

    private async Task<IStorageFolder?> GetStorageFolderAsync(WowVersions wowVersion, string folder)
    {
        if (_installedVersions.Contains(wowVersion))
        {
            Console.WriteLine($"Getting {_installDir}{wowVersion.StringValue()}/{folder}");
            return await _storageProvider.TryGetFolderFromPathAsync($"{_installDir}{wowVersion.StringValue()}/{folder}");
        }
        return null;
    }

    public async Task<List<IStorageItem>?> GetAddons(WowVersions wowVersion)
    {
        var items = (await GetStorageFolderAsync(wowVersion, "Interface/AddOns"))?.GetItemsAsync().ToListAsync().AsTask();
        return items == null ? null : await items;
    }

    public async Task<List<IStorageItem>?> GetWTF(WowVersions wowVersion)
    {
        var items = (await GetStorageFolderAsync(wowVersion, "WTF/Account"))?.GetItemsAsync().ToListAsync().AsTask();
        return items == null ? null : await items;
    }
}