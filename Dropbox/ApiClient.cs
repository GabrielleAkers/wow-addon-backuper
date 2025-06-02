using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dropbox;

public class ApiClient(BearerToken token)
{
    private readonly static string _api_base_url = "https://api.dropboxapi.com/2";
    private readonly BearerToken _token = token;

    private async Task<HttpRequestMessage> MakePost(string url, bool use_auth, HttpContent? content, string content_type)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url);
        if (use_auth)
        {
            if (OAuthHandler.Instance().AccessTokenExpireTimeDiff() < 60)
                await OAuthHandler.Instance().RefreshToken(_token);
            req.Headers.Add("Authorization", $"Bearer {OAuthHandler.Instance().BearerToken.AccessToken}");
        }
        if (content != null)
        {
            req.Content = content;
            req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(content_type);
        }
        return req;
    }

    public async Task CheckUser()
    {
        var req = await MakePost(
            $"{_api_base_url}/check/user",
            true,
            new StringContent(JsonSerializer.Serialize(new Dictionary<string, string> { { "query", "foo" } }, KeyValueJsonContext.Default.KeyValueDict)),
            MediaTypeNames.Application.Json);
        var res = await Http.client.SendAsync(req);
        res.EnsureSuccessStatusCode();
        Console.WriteLine("User is good :)");
    }

    public async Task<UserAccountInfo?> GetAccount()
    {
        var req = await MakePost(
            $"{_api_base_url}/users/get_account",
            true,
            new StringContent(JsonSerializer.Serialize(new Dictionary<string, string> { { "account_id", _token.AccountId! } }, KeyValueJsonContext.Default.KeyValueDict)),
            MediaTypeNames.Application.Json);
        var res = await Http.client.SendAsync(req);
        res.EnsureSuccessStatusCode();

        var content = await res.Content.ReadFromJsonAsync(ApiDataJsonContext.Default.UserAccountInfo);
        return content;
    }
}