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
    public BearerToken BearerToken { get; private set; }

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

    private async Task AccessTokenRequest(string auth_code, string redirect_uri)
    {
        Console.WriteLine("Requesting new access token");
        var post_res = await Http.client.PostAsync($"{_api_base_url}/oauth2/token?client_id={_client_id}&redirect_uri={redirect_uri}&code_verifier={_code_verifier}&grant_type=authorization_code&code={auth_code}", null);
        if (!post_res.IsSuccessStatusCode) throw new Exception(post_res.ReasonPhrase);

        var response = await post_res.Content.ReadFromJsonAsync(TokenResponseJsonContext.Default.AuthTokenResponse) ?? throw new Exception("Failed to fetch bearer token");
        BearerToken.AccessToken = response.AccessToken;
        BearerToken.ExpiresAt = DateTimeOffset.Now.ToUnixTimeSeconds() + response.ExpiresIn;
        BearerToken.RefreshToken = response.RefreshToken;
        BearerToken.AccountId = response.AccountId;
    }

    private async Task RefreshTokenRequest()
    {
        Console.WriteLine("Refreshing token");
        var post_res = await Http.client.PostAsync($"{_api_base_url}/oauth2/token?client_id={_client_id}&refresh_token={BearerToken.RefreshToken}&grant_type=refresh_token", null);
        if (!post_res.IsSuccessStatusCode) throw new Exception(post_res.ReasonPhrase);

        var response = await post_res.Content.ReadFromJsonAsync(TokenResponseJsonContext.Default.RefreshTokenResponse) ?? throw new Exception("Failed to refresh token");
        BearerToken.AccessToken = response.AccessToken!;
        BearerToken.ExpiresAt = DateTimeOffset.Now.ToUnixTimeSeconds() + response.ExpiresIn;
    }

    private async Task WriteRefreshTokenToFile()
    {
        if (string.IsNullOrEmpty(BearerToken.RefreshToken) || string.IsNullOrEmpty(BearerToken.AccountId))
        {
            Console.WriteLine("No refresh token/account id");
            return;
        }
        Console.WriteLine("Writing refresh token to file");

        var dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/wow-addon-backuper";
        Directory.CreateDirectory(dir);
        var file = $"{dir}/user.data";
        using (var fs = File.OpenWrite(file))
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string> { { "RefreshToken", BearerToken.RefreshToken }, { "AccountId", BearerToken.AccountId } }, KeyValueJsonContext.Default.KeyValueDict);
            await fs.WriteAsync(bytes);
        }
    }

    private async Task ReadRefreshTokenFromFile()
    {
        var dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/wow-addon-backuper";
        var file = $"{dir}/user.data";
        if (!File.Exists(file)) return;

        var finfo = new FileInfo(file);
        byte[] buffer = new byte[finfo.Length];
        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length, true))
        {
            while (true)
            {
                if ((await fs.ReadAsync(buffer)) <= 0)
                    break;
            }
            var kv = JsonSerializer.Deserialize(buffer, KeyValueJsonContext.Default.KeyValueDict);
            if (kv == null) return;
            BearerToken.RefreshToken = kv["RefreshToken"];
            BearerToken.AccountId = kv["AccountId"];
        }
    }

    public async Task SignIn(BearerToken bearer)
    {
        BearerToken = bearer;
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
            var result = AuthorizeResponse.ParseRequestURL(context.Request.RawUrl, _state!);
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

            await AccessTokenRequest(result.Code!, redirect_uri);
            Console.WriteLine($"Successfully signed in as account {BearerToken.AccountId}");
            await WriteRefreshTokenToFile();
        }
        finally
        {
            Cleanup();
            http.Close();
        }
    }

    public async Task RefreshToken(BearerToken bearer)
    {
        BearerToken = bearer;
        if (string.IsNullOrEmpty(BearerToken.RefreshToken))
        {
            await Task.Run(ReadRefreshTokenFromFile);
        }
        if (string.IsNullOrEmpty(BearerToken.RefreshToken))
        {
            Console.WriteLine("No refresh token in file or memory");
            return;
        }

        Console.WriteLine("Refreshing token");
        await RefreshTokenRequest();
        await WriteRefreshTokenToFile();
    }

    public long AccessTokenExpireTimeDiff()
    {
        return BearerToken.ExpiresAt - DateTimeOffset.Now.ToUnixTimeSeconds();
    }
}