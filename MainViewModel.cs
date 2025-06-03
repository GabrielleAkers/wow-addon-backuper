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
    private Dropbox.UserAccountInfo? _userAccountInfo;

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
}