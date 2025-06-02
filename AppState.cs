using CommunityToolkit.Mvvm.ComponentModel;

namespace wow_addon_backuper;

public partial class AppState(Dropbox.BearerToken token) : ObservableObject
{
    [ObservableProperty]
    private Dropbox.BearerToken _token = token;

    [ObservableProperty]
    private Dropbox.UserAccountInfo? _userAccountInfo;
}