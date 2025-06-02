using CommunityToolkit.Mvvm.ComponentModel;

namespace Dropbox;

public partial class BearerToken : ObservableObject
{
    [ObservableProperty]
    private string? _accessToken;
    [ObservableProperty]
    private long _expiresAt;
    [ObservableProperty]
    private string? _refreshToken;
    [ObservableProperty]
    private string? _accountId;
}