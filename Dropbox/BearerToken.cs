using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Dropbox;

public partial class BearerToken : ObservableObject
{
    [ObservableProperty]
    private string? _accessToken;
    [ObservableProperty]
    private long? _expiresAt;
    [ObservableProperty]
    private string? _refreshToken;
    [ObservableProperty]
    private string? _accountId;

    public void Clear()
    {
        AccessToken = null;
        ExpiresAt = null;
        RefreshToken = null;
        AccountId = null;
    }

    public long AccessTokenExpireTimeDiff()
    {
        if (ExpiresAt == null) return 0;

        return (long)(ExpiresAt - DateTimeOffset.Now.ToUnixTimeSeconds());
    }
}