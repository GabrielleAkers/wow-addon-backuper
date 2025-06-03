using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace wow_addon_backuper;

public partial class MainViewModel(Dropbox.BearerToken token) : ObservableObject
{
    [ObservableProperty]
    private Dropbox.BearerToken _dropboxToken = token;

    [ObservableProperty]
    private Dropbox.Responses.UserAccountInfo? _userAccountInfo;

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
}