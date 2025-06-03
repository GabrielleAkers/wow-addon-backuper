using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using wow_addon_backuper;

namespace Dropbox;

class OAuthHandler
{
    private readonly string _base_url = "https://www.dropbox.com";
    private readonly string _api_base_url = "https://api.dropbox.com";
    private readonly string _client_id = "notk0buol9w5mgm";
    private static OAuthHandler? _instance = null;
    private static readonly Lock _mutex = new();
    private OAuthHandler() { }

    public static OAuthHandler Instance()
    {
        if (_instance == null)
        {
            lock (_mutex)
            {
                _instance ??= new OAuthHandler();
            }
        }
        return _instance;
    }

    private string? _code_verifier;
    private string? _state;

    private void GenerateCodeVerifier()
    {
        char[] allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~".ToCharArray();
        _code_verifier = RandomNumberGenerator.GetString(allowed, 128);
    }

    private void GenerateState()
    {
        char[] allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~".ToCharArray();
        _state = RandomNumberGenerator.GetString(allowed, 128);
    }

    private void PrepareForAuthorize()
    {
        GenerateCodeVerifier();
        GenerateState();
    }

    private void Cleanup()
    {
        _code_verifier = null;
        _state = null;
    }

    private string GetCodeChallenge()
    {
        if (_code_verifier == null)
        {
            throw new Exception("Code verifier is null.");
        }

        byte[] s256_verifier = SHA256.HashData(Encoding.UTF8.GetBytes(_code_verifier));
        return Base64Url.EncodeToString(s256_verifier);
    }

    private void Authorize(string redirect_uri)
    {
        string challenge_code = GetCodeChallenge();
        string response_type = "code";
        string code_challenge_method = "S256";
        string token_access_type = "offline";
        Process.Start(new ProcessStartInfo($"{_base_url}/oauth2/authorize?client_id={_client_id}&redirect_uri={redirect_uri}&response_type={response_type}&code_challenge={challenge_code}&code_challenge_method={code_challenge_method}&token_access_type={token_access_type}&state={_state}") { UseShellExecute = true });
    }

    private async Task AccessTokenRequest(BearerToken token, string auth_code, string redirect_uri)
    {
        Console.WriteLine("Requesting new access token");
        var post_res = await Http.client.PostAsync($"{_api_base_url}/oauth2/token?client_id={_client_id}&redirect_uri={redirect_uri}&code_verifier={_code_verifier}&grant_type=authorization_code&code={auth_code}", null);
        if (!post_res.IsSuccessStatusCode) throw new Exception(post_res.ReasonPhrase);

        var response = await post_res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.AuthToken) ?? throw new Exception("Failed to fetch bearer token");
        token.AccessToken = response.AccessToken;
        token.ExpiresAt = DateTimeOffset.Now.ToUnixTimeSeconds() + response.ExpiresIn;
        token.RefreshToken = response.RefreshToken;
        token.AccountId = response.AccountId;
    }

    private async Task RefreshTokenRequest(BearerToken token)
    {
        Console.WriteLine("Refreshing token");
        var post_res = await Http.client.PostAsync($"{_api_base_url}/oauth2/token?client_id={_client_id}&refresh_token={token.RefreshToken}&grant_type=refresh_token", null);
        if (!post_res.IsSuccessStatusCode) throw new Exception(post_res.ReasonPhrase);

        var response = await post_res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.RefreshToken) ?? throw new Exception("Failed to refresh token");
        token.AccessToken = response.AccessToken!;
        token.ExpiresAt = DateTimeOffset.Now.ToUnixTimeSeconds() + response.ExpiresIn;
    }

    private static async Task WriteUserDataToFile(BearerToken token)
    {
        if (string.IsNullOrEmpty(token.RefreshToken) || string.IsNullOrEmpty(token.AccountId))
        {
            Console.WriteLine("No refresh token/account id");
            return;
        }
        Console.WriteLine("Writing refresh token to file");

        await Util.WriteToFile("user.data", JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string> { { "RefreshToken", token.RefreshToken }, { "AccountId", token.AccountId } }, KeyValueJsonContext.Default.KeyValueDict));
    }

    private static async Task ReadUserDataFromFile(BearerToken token)
    {
        var buffer = await Util.ReadFromFile("user.data");
        var kv = JsonSerializer.Deserialize(buffer, KeyValueJsonContext.Default.KeyValueDict);
        if (kv == null) return;
        token.RefreshToken = kv["RefreshToken"];
        token.AccountId = kv["AccountId"];
    }

    public async Task SignIn(BearerToken token)
    {
        int port = 41685;
        string redirect_uri = $"http://localhost:{port}/";
        var http = new HttpListener();
        http.Prefixes.Add(redirect_uri);
        Console.WriteLine($"Listening on {redirect_uri}");
        http.Start();

        try
        {
            PrepareForAuthorize();
            Authorize(redirect_uri);

            var context = await http.GetContextAsync().WaitAsync(TimeSpan.FromSeconds(30));
            var result = Responses.Authorize.ParseRequestURL(context.Request.RawUrl, _state!);
            if (result.IsError)
                throw new Exception(result.ErrorDescription);
            if (result.State != _state)
                throw new Exception("State mismatch");

            var response = context.Response;
            string html = $"<!DOCTYPE html><html><head><meta content='url={redirect_uri}'</head><body style='background-color: #1d2130; color: white;'><div style='margin: auto; text-align: center; padding: 70px 0;'><h1>Go back to the app NOW &#128511;</h1></div></body></html>";
            var buffer = Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            var response_output = response.OutputStream;
            await response_output.WriteAsync(buffer);
            response_output.Close();

            await AccessTokenRequest(token, result.Code!, redirect_uri);
            Console.WriteLine($"Successfully signed in as account {token.AccountId}");
            await WriteUserDataToFile(token);
        }
        finally
        {
            Cleanup();
            http.Close();
        }
    }

    public void SignOut()
    {
        Util.DeleteFile("user.data");
    }

    public async Task RefreshToken(BearerToken token)
    {
        if (string.IsNullOrEmpty(token.RefreshToken))
        {
            async Task t() => await ReadUserDataFromFile(token);
            await Task.Run(t);
        }
        if (string.IsNullOrEmpty(token.RefreshToken))
        {
            Console.WriteLine("No refresh token in file or memory");
            return;
        }

        Console.WriteLine("Refreshing token");
        await RefreshTokenRequest(token);
        await WriteUserDataToFile(token);
    }
}