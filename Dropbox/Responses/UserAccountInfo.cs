using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Dropbox.Responses;

public partial class AccountName : ObservableObject
{
    [ObservableProperty]
    [JsonPropertyName("abbreviated_name")]
    public partial string? AbbreviatedName { get; set; }

    [ObservableProperty]
    [JsonPropertyName("display_name")]
    public partial string? DisplayName { get; set; }

    [ObservableProperty]
    [JsonPropertyName("familiar_name")]
    public partial string? FamiliarName { get; set; }

    [ObservableProperty]
    [JsonPropertyName("given_name")]
    public partial string? GivenName { get; set; }

    [ObservableProperty]
    [JsonPropertyName("surname")]
    public partial string? Surname { get; set; }
}

public partial class UserAccountInfo : ObservableObject
{
    [ObservableProperty]
    [JsonPropertyName("name")]
    public partial AccountName? Name { get; set; }

    [ObservableProperty]
    [JsonPropertyName("profile_photo_url")]
    public partial string? ProfilePhotoUrl { get; set; }
}