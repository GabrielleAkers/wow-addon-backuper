using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Dropbox.Requests;

namespace Dropbox;

public class ApiClient(BearerToken token)
{
    private readonly static string _api_base_url = "https://api.dropboxapi.com/2";
    private readonly BearerToken _token = token;

    private async Task<HttpRequestMessage> MakePost(string url, bool use_auth, HttpContent? content, string? content_type)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url);
        if (use_auth)
        {
            if (_token.AccessTokenExpireTimeDiff() < 60)
                await OAuthHandler.Instance().RefreshToken(_token);
            req.Headers.Add("Authorization", $"Bearer {_token.AccessToken}");
        }
        if (content != null && !string.IsNullOrEmpty(content_type))
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
            new CheckUser { Query = "foo" }.ToHttpContent(DropboxRequestJsonContext.Default.CheckUser),
            MediaTypeNames.Application.Json);
        var res = await Http.client.SendAsync(req);
        res.EnsureSuccessStatusCode();
        Console.WriteLine("User is good :)");
    }

    public async Task<Responses.UserAccountInfo?> GetAccount()
    {
        var req = await MakePost(
            $"{_api_base_url}/users/get_account",
            true,
            new GetAccount { AccountId = _token.AccountId! }.ToHttpContent(DropboxRequestJsonContext.Default.GetAccount),
            MediaTypeNames.Application.Json);
        var res = await Http.client.SendAsync(req);
        res.EnsureSuccessStatusCode();

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.UserAccountInfo);
        return content;
    }

    public async Task RevokeToken()
    {
        var req = await MakePost(
            $"{_api_base_url}/auth/token/revoke",
            true,
            null,
            null
        );
        await Http.client.SendAsync(req);
    }

    public async Task<Responses.ListFolder?> ListFolder(string path)
    {
        var req = await MakePost(
            $"{_api_base_url}/files/list_folder",
            true,
            new ListFolder { Path = path }.ToHttpContent(DropboxRequestJsonContext.Default.ListFolder),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        var s = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
            throw new Exception(s);

        Console.WriteLine(s);

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.ListFolder);
        return content;
    }

    public async Task<Responses.CreateFolder?> CreateFolder(string path)
    {
        var req = await MakePost(
            $"{_api_base_url}/files/create_folder_v2",
            true,
            new CreateFolder { Path = path }.ToHttpContent(DropboxRequestJsonContext.Default.CreateFolder),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        var s = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
            throw new Exception(s);

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.CreateFolder);
        return content;
    }
}