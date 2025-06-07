using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace wow_addon_backuper;

public partial class MainViewModel : ObservableObject
{
    private readonly WrappedConsole _logger = WrappedConsole.Instance();
    public Window? MainWindow;

    public MainViewModel(Dropbox.BearerToken token)
    {
        _logger.Changed += UpdateLogOutput;
        _dropboxToken = token;

        // special handling for nested property change in SyncedSetting
        WowInstallDir.PropertyChanged += HandleWowInstallDirChanged;
        PropertyChanged += OnPropertyChanged;
    }

    #region ChangeHandlers

    public void HandleWindowLoaded()
    {
        // hack to update view on initial load, called after Window is constructed by app
        HandleWowInstallDirChanged(WowInstallDir, new PropertyChangedEventArgs(nameof(WowInstallDir)));
    }

    private void UpdateLogOutput(object sender, string? value)
    {
        if (value != null)
        {
            LogMessages.Add(value);
            SelectedLogMessage = LogMessages.Count - 1;
        }
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedGameVersionIndex):
                await HandleSelectedGameVersionIndexChanged();
                break;
            case nameof(ShowDropboxItems):
                await HandleSelectedGameVersionIndexChanged();
                break;
            case nameof(SelectedAddonsOrAccountChoicesIndex):
                if (sender == null)
                    break;
                HandleSelectedAddonsOrAccountIndexChanged();
                break;
            case nameof(SelectAllAddonsOrAccountRows):
                if (sender == null)
                    break;
                HandleSelectAllAddonsOrAccountRowsChanged();
                break;
            default:
                break;
        }
    }

    private void HandleSelectAllAddonsOrAccountRowsChanged()
    {
        var t = AddonsOrAccountFolderDataRows.ToList();
        t.ForEach(r => r.IsSelected = SelectAllAddonsOrAccountRows);
        AddonsOrAccountFolderDataRows = new ObservableCollection<AddonsOrAccountFolderDataRow>(t);
    }

    private void HandleWowInstallDirChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "WowInstallDir" && sender != null)
        {
            var install_dir = (sender as SyncedSetting<string>)!.Value;
            if (install_dir == null) return;
            _logger.WriteLine($"Checking {install_dir} for game versions");
            var wow_vers = Enum.GetValues<WowVersions>();
            for (var i = 0; i < wow_vers.Length; i++)
            {
                var dir = $"{install_dir}{wow_vers[i].StringValue()}";
                _logger.WriteLine($"Checking {dir}");
                if (Directory.Exists(dir))
                {
                    _logger.WriteLine($"Found version {wow_vers[i]}");
                    if (!InstalledGameVersions.Contains(wow_vers[i]))
                        InstalledGameVersions.Add(wow_vers[i]);
                }
            }
            OnPropertyChanged(nameof(SelectedGameVersionIndex));
        }
    }

    private async Task HandleSelectedGameVersionIndexChanged()
    {
        var storage = MainWindow?.StorageProvider;
        if (storage == null || WowInstallDir.Value == null) return;

        var wowStorage = new WowStorageHandler(storage, WowInstallDir.Value, InstalledGameVersions);

        List<FileOrFolderData>? addons;
        List<FileOrFolderData>? accounts;

        if (ShowDropboxItems)
        {
            addons = await wowStorage.GetAddons(InstalledGameVersions[SelectedGameVersionIndex], true);
            accounts = await wowStorage.GetWTF(InstalledGameVersions[SelectedGameVersionIndex], true);
        }
        else
        {
            addons = await wowStorage.GetAddons(InstalledGameVersions[SelectedGameVersionIndex]);
            accounts = await wowStorage.GetWTF(InstalledGameVersions[SelectedGameVersionIndex]);
        }
        SelectedGameVersionAddons = addons ?? [];
        SelectedGameVersionWTF = accounts ?? [];
        OnPropertyChanged(nameof(SelectedAddonsOrAccountChoicesIndex));
        SelectAllAddonsOrAccountRows = false;
    }

    private void HandleSelectedAddonsOrAccountIndexChanged()
    {
        var choice = Enum.GetValues<AddonsOrAccount>().ToList().Find(e => e.StringValue() == AddonsOrAccountChoices[SelectedAddonsOrAccountChoicesIndex]);
        List<AddonsOrAccountFolderDataRow> dataRows = [];
        switch (choice)
        {
            case AddonsOrAccount.AddOns:
                SelectedGameVersionAddons.ForEach(s =>
                {
                    var p = s.Path;
                    if (p == null) return;
                    dataRows.Add(new AddonsOrAccountFolderDataRow { IsSelected = false, Path = p, Name = s.Name, RemotePath = s.RemotePath });
                });
                break;
            case AddonsOrAccount.Account:
                SelectedGameVersionWTF.ForEach(s =>
                {
                    var p = s.Path;
                    if (p == null) return;
                    dataRows.Add(new AddonsOrAccountFolderDataRow { IsSelected = false, Path = p, Name = s.Name, RemotePath = s.RemotePath });
                });
                break;
            default:
                break;
        }
        dataRows.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
        AddonsOrAccountFolderDataRows = new ObservableCollection<AddonsOrAccountFolderDataRow>(dataRows);
        SelectAllAddonsOrAccountRows = false;
    }

    #endregion

    #region ObservableProperties

    [ObservableProperty]
    private Dropbox.BearerToken _dropboxToken;

    [ObservableProperty]
    private Dropbox.Responses.UserAccountInfo? _userAccountInfo;

    [ObservableProperty]
    private SyncedSetting<string> _wowInstallDir = new("WowInstallDir", "app-settings.data");

    [ObservableProperty]
    private List<WowVersions> _installedGameVersions = [];

    [ObservableProperty]
    private int _selectedGameVersionIndex = 0;

    [ObservableProperty]
    private List<FileOrFolderData> _selectedGameVersionAddons = [];

    [ObservableProperty]
    private List<FileOrFolderData> _selectedGameVersionWTF = [];

    [ObservableProperty]
    private List<string> _addonsOrAccountChoices = [.. Enum.GetValues<AddonsOrAccount>().Select(e => e.StringValue())];

    [ObservableProperty]
    private int _selectedAddonsOrAccountChoicesIndex = 0;

    [ObservableProperty]
    private ObservableCollection<AddonsOrAccountFolderDataRow> _addonsOrAccountFolderDataRows = [];

    [ObservableProperty]
    private bool _selectAllAddonsOrAccountRows;

    [ObservableProperty]
    private bool _anyIsLoading;

    [ObservableProperty]
    private ObservableCollection<string> _logMessages = [];

    [ObservableProperty]
    private int _selectedLogMessage;

    [ObservableProperty]
    private bool _showDropboxItems;

    #endregion

    #region Commands

    [RelayCommand]
    private async Task DropboxSignin()
    {
        try
        {
            await Dropbox.OAuthHandler.Instance().SignIn(App.AppState.DropboxToken);
            App.AppState.UserAccountInfo = await App.DropboxApi.GetAccount();
        }
        catch (Exception e)
        {
            _logger.WriteLine(e.Message);
        }
    }

    [RelayCommand]
    private async Task DropboxSignout()
    {
        try
        {
            await App.DropboxApi.RevokeToken();
        }
        catch (Exception e)
        {
            _logger.WriteLine(e.Message);
        }
        finally
        {
            Dropbox.OAuthHandler.Instance().SignOut();
            App.AppState.DropboxToken.Clear();
            App.AppState.UserAccountInfo = null;
        }
    }

    [RelayCommand]
    private async Task PickWowDir()
    {
        try
        {
            var storage = MainWindow?.StorageProvider;
            if (storage != null && storage.CanPickFolder)
            {
                var picked_folder = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false, Title = "Pick WoW install directory" });
                if (picked_folder == null) return;
                if (picked_folder.Count <= 0) return;
                WowInstallDir.Value = $"{picked_folder[0].Path.AbsolutePath.Replace("%20", " ")}";
            }
        }
        catch (Exception e)
        {
            _logger.WriteLine(e.Message);
        }
    }

    [RelayCommand]
    private async Task UploadSelected()
    {
        var selected = AddonsOrAccountFolderDataRows.ToList().FindAll(r => r.IsSelected);
        for (var i = 0; i < AddonsOrAccountFolderDataRows.Count; i++)
        {
            var folder = AddonsOrAccountFolderDataRows[i];
            if (!folder.IsSelected) continue;
            folder.IsLoading = true;
            AddonsOrAccountFolderDataRows[i] = folder;
            AnyIsLoading = true;
            try
            {
                await App.DropboxApi.UploadFolderZipped(
                        $"{folder.Path}",
                        $"{Enum.GetName(InstalledGameVersions[SelectedGameVersionIndex])}/{AddonsOrAccountChoices[SelectedAddonsOrAccountChoicesIndex]}"
                    );
            }
            catch (Exception e)
            {
                _logger.WriteLine(e.Message);
            }
            folder.IsLoading = false;
            AddonsOrAccountFolderDataRows[i] = folder;
        }
        AnyIsLoading = false;
    }

    [RelayCommand]
    private async Task DownloadSelected()
    {
        var selected = AddonsOrAccountFolderDataRows.ToList().FindAll(r => r.IsSelected);
        for (var i = 0; i < AddonsOrAccountFolderDataRows.Count; i++)
        {
            var folder = AddonsOrAccountFolderDataRows[i];
            if (!folder.IsSelected) continue;
            folder.IsLoading = true;
            AddonsOrAccountFolderDataRows[i] = folder;
            AnyIsLoading = true;
            try
            {
                await App.DropboxApi.DownloadFolderZipped(
                        $"{folder.Path}",
                        $"{folder.RemotePath}"
                    );
            }
            catch (Exception e)
            {
                _logger.WriteLine(e.Message);
            }
            folder.IsLoading = false;
            AddonsOrAccountFolderDataRows[i] = folder;
        }
        AnyIsLoading = false;
    }
    #endregion
}