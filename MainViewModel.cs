using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace wow_addon_backuper;

public partial class MainViewModel : ObservableObject
{
    public Window? MainWindow;

    public MainViewModel(Dropbox.BearerToken token)
    {
        _dropboxToken = token;

        WowInstallDir.PropertyChanged += HandleWowInstallDirChanged;
    }

    private void HandleWowInstallDirChanged(object? sender, PropertyChangedEventArgs e)
    {
        Console.WriteLine($"Prop changed {e.PropertyName}");
        if (e.PropertyName == "WowInstallDir" && sender != null)
        {
            var install_dir = (sender as SyncedSetting<string>)!.Value;
            if (install_dir == null) return;
            Console.WriteLine($"Checking {install_dir} for game versions");
            var wow_vers = Enum.GetValues<WowVersions>();
            for (var i = 0; i < wow_vers.Length; i++)
            {
                var dir = $"{install_dir}{wow_vers[i].StringValue()}";
                Console.WriteLine($"Checking {dir}");
                if (Directory.Exists(dir))
                {
                    Console.WriteLine($"Found version {wow_vers[i]}");
                    InstalledGameVersions.Add(wow_vers[i].SeparateWords());
                }
            }
        }
    }

    [ObservableProperty]
    private Dropbox.BearerToken _dropboxToken;

    [ObservableProperty]
    private Dropbox.Responses.UserAccountInfo? _userAccountInfo;

    [ObservableProperty]
    private SyncedSetting<string> _wowInstallDir = new("WowInstallDir", "app-settings.data");

    [ObservableProperty]
    private List<string> _installedGameVersions = new();

    [ObservableProperty]
    private int _selectedGameVersionIndex = 0;

    [RelayCommand]
    public static async Task DropboxSignin()
    {
        await Dropbox.OAuthHandler.Instance().SignIn(App.AppState.DropboxToken);
        App.AppState.UserAccountInfo = await App.DropboxApi.GetAccount();
    }

    [RelayCommand]
    public static async Task DropboxSignout()
    {
        await App.DropboxApi.RevokeToken();
        Dropbox.OAuthHandler.Instance().SignOut();
        App.AppState.DropboxToken.Clear();
        App.AppState.UserAccountInfo = null;
    }

    [RelayCommand]
    public static async Task DropboxListFolder(string path)
    {
        await App.DropboxApi.ListFolder(path);
    }

    [RelayCommand]
    public static async Task DropboxCheckUser()
    {
        await App.DropboxApi.CheckUser();
    }

    [RelayCommand]
    public static async Task DropboxCreateFolder(string path)
    {
        await App.DropboxApi.CreateFolder(path);
    }

    [RelayCommand]
    private async Task PickWowDir()
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
}