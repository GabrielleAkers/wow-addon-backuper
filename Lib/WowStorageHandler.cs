using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace wow_addon_backuper;

class WowStorageHandler(IStorageProvider storageProvider, string installDir, List<WowVersions> installedVersions)
{
    private readonly WrappedConsole _logger = WrappedConsole.Instance();
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

    private async Task<List<Dropbox.Responses.FolderEntry>?> GetRemoteFolderHelper(string folder)
    {
        Dropbox.Responses.ListFolder? listResponse;
        try
        {
            listResponse = await App.DropboxApi.ListFolder($"/{folder}");
            if (listResponse == null)
            {
                _logger.WriteLine($"No remote {folder}");
                return null;
            }
            if (!string.IsNullOrEmpty(listResponse.ErrorSummary))
            {
                if (listResponse.ErrorSummary == "path/not_found/")
                {
                    _logger.WriteLine($"No remote {folder}");
                    return null;
                }
                else
                {
                    throw new Exception(listResponse.ToString());
                }
            }
        }
        catch (Exception e)
        {
            if (e.Message.Contains("path/not_found/"))
            {
                _logger.WriteLine($"No remote {folder}");
            }
            else
            {
                _logger.WriteLine(e.Message);
            }
            return null;
        }
        var remoteItems = listResponse.Entries ?? [];
        var hasMore = listResponse.HasMore;
        var mostRecentCursor = listResponse.Cursor;
        while (hasMore && !string.IsNullOrEmpty(mostRecentCursor))
        {
            var t = await App.DropboxApi.ListFolderContinue(mostRecentCursor);
            if (t != null)
            {
                if (t.Entries == null) break;
                remoteItems.AddRange(t.Entries);
                hasMore = t.HasMore;
                mostRecentCursor = t.Cursor;
            }
            _logger.WriteLine("ERROR: List response indicated there were more entries but none were return in continue");
            break;
        }
        return remoteItems;
    }

    public async Task<List<FileOrFolderData>?> GetAddons(WowVersions wowVersion, bool remote = false)
    {
        if (remote)
        {
            _logger.WriteLine($"Getting remote:/{wowVersion}/AddOns");
            var remoteAddons = await GetRemoteFolderHelper($"{wowVersion}/AddOns");
            return remoteAddons == null ? null : [.. remoteAddons.Select(s =>
                new FileOrFolderData {
                    Name = s.Name?.Split(".zip")[0] ?? s.Name,
                    Path = $"{_installDir}{wowVersion.StringValue()}/Interface/AddOns/" + s.Name?.Split(".zip")[0] ?? s.Name,
                    RemotePath = s.PathDisplay
                })
            ];
        }
        else
        {
            var task = (await GetStorageFolderAsync(wowVersion, "Interface/AddOns"))?.GetItemsAsync().ToListAsync().AsTask();
            if (task == null) return null;
            var t = await task;
            return t == null ? null : [.. t.Select(s => new FileOrFolderData {
                    Name = s.Name,
                    Path = s.TryGetLocalPath(),
                    RemotePath = $"{wowVersion}/AddOns/" + s.Name
                })
            ];
        }
    }

    public async Task<List<FileOrFolderData>?> GetWTF(WowVersions wowVersion, bool remote = false)
    {
        if (remote)
        {
            _logger.WriteLine($"Getting remote:/{wowVersion}/Account Settings");
            var remoteWTF = await GetRemoteFolderHelper($"{wowVersion}/Account Settings");
            return remoteWTF == null ? null : [.. remoteWTF.Select(s =>
                new FileOrFolderData {
                    Name = s.Name?.Split(".zip")[0] ?? s.Name,
                    Path = $"{_installDir}{wowVersion.StringValue()}/WTF/Account/" + s.Name?.Split(".zip")[0] ?? s.Name,
                    RemotePath = s.PathDisplay,
                })
            ];
        }
        else
        {
            var items = (await GetStorageFolderAsync(wowVersion, "WTF/Account"))?.GetItemsAsync().ToListAsync().AsTask();
            if (items == null) return null;
            var t = await items;
            return t == null ? null : [.. t.Select(s => new FileOrFolderData {
                    Name = s.Name,
                    Path = s.TryGetLocalPath(),
                    RemotePath = $"{wowVersion}/Account Settings/" + s.Name
                })
            ];
        }
    }
}