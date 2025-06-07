using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Threading.Tasks;
using wow_addon_backuper;

namespace Dropbox;

public class ApiClient(BearerToken token)
{
    private WrappedConsole _logger = WrappedConsole.Instance();
    private readonly static string _api_base_url = "https://api.dropboxapi.com/2";
    private readonly static string _content_base_url = "https://content.dropboxapi.com/2";
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
            new Requests.CheckUser { Query = "foo" }.ToStringContent(DropboxRequestJsonContext.Default.CheckUser),
            MediaTypeNames.Application.Json);
        var res = await Http.client.SendAsync(req);
        res.EnsureSuccessStatusCode();
        _logger.WriteLine("User is good :)");
    }

    public async Task<Responses.UserAccountInfo?> GetAccount()
    {
        var req = await MakePost(
            $"{_api_base_url}/users/get_account",
            true,
            new Requests.GetAccount { AccountId = _token.AccountId! }.ToStringContent(DropboxRequestJsonContext.Default.GetAccount),
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
            new Requests.ListFolder { Path = path }.ToStringContent(DropboxRequestJsonContext.Default.ListFolder),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.ListFolder);
        return content;
    }

    public async Task<Responses.ListFolder?> ListFolderContinue(string cursor)
    {
        var req = await MakePost(
            $"{_api_base_url}/files/list_folder/continue",
            true,
            new Requests.ListFolderContinue { Cursor = cursor }.ToStringContent(DropboxRequestJsonContext.Default.ListFolderContinue),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.ListFolder);
        return content;
    }

    public async Task<Responses.CreateFolder?> CreateFolder(string path)
    {
        var req = await MakePost(
            $"{_api_base_url}/files/create_folder_v2",
            true,
            new Requests.CreateFolder { Path = path }.ToStringContent(DropboxRequestJsonContext.Default.CreateFolder),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.CreateFolder);
        return content;
    }

    public async Task<Responses.FileMetadata?> GetMetadata(string path)
    {
        var req = await MakePost(
            $"{_api_base_url}/files/get_metadata",
            true,
            new Requests.GetMetadata { Path = path }.ToStringContent(DropboxRequestJsonContext.Default.GetMetadata),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.FileMetadata);
        return content;
    }

    public async Task<Responses.FileMetadata?> Upload(byte[] bytes, string path, int? delay = null)
    {
        if (delay != null)
            await Task.Delay((int)delay);

        var req = await MakePost(
            $"{_content_base_url}/files/upload",
            true,
            new ByteArrayContent(bytes),
            MediaTypeNames.Application.Octet
        );
        req.Headers.Add("Dropbox-API-Arg", new Requests.Upload { Path = path, Mode = "overwrite" }.ToJson(DropboxRequestJsonContext.Default.Upload));
        var res = await Http.client.SendAsync(req);
        if (res.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            var retry_after = res.Headers.RetryAfter?.Delta;
            return await Upload(bytes, path, retry_after == null ? 1000 : (int)retry_after.Value.TotalMilliseconds);
        }
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.FileMetadata);
        return content;
    }

    public async Task<Responses.SessionStart?> UploadSessionStart(byte[] bytes)
    {
        var req = await MakePost(
            $"{_content_base_url}/files/upload_session/start",
            true,
            new ByteArrayContent(bytes),
            MediaTypeNames.Application.Octet
        );
        req.Headers.Add("Dropbox-API-Arg", new Requests.UploadSessionStart { Close = false }.ToJson(DropboxRequestJsonContext.Default.UploadSessionStart));
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.SessionStart);
        return content;
    }

    public async Task UploadSessionAppend(byte[] bytes, long offset, string sessionId)
    {
        var req = await MakePost(
            $"{_content_base_url}/files/upload_session/append_v2",
            true,
            new ByteArrayContent(bytes),
            MediaTypeNames.Application.Octet
        );
        req.Headers.Add("Dropbox-API-Arg",
            new Requests.UploadSessionAppend
            {
                Cursor = new Requests.UploadSessionCursor { Offset = offset, SessionId = sessionId }
            }
                .ToJson(DropboxRequestJsonContext.Default.UploadSessionAppend));
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());
    }

    public async Task<Responses.FileMetadata?> UploadSessionFinish(byte[] bytes, long offset, string sessionId, string path)
    {
        var req = await MakePost(
            $"{_content_base_url}/files/upload_session/finish",
            true,
            new ByteArrayContent(bytes),
            MediaTypeNames.Application.Octet
        );
        req.Headers.Add("Dropbox-API-Arg",
            new Requests.UploadSessionFinish
            {
                Cursor = new Requests.UploadSessionCursor { Offset = offset, SessionId = sessionId },
                Commit = new Requests.UploadSessionCommit { Path = path }
            }
                .ToJson(DropboxRequestJsonContext.Default.UploadSessionAppend));
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.FileMetadata);
        return content;
    }

    public async Task<Responses.BatchSessionStart?> UploadSessionBatchStart(int numSessions)
    {
        var req = await MakePost(
            $"{_api_base_url}/files/upload_session/start_batch",
            true,
            new Requests.UploadSessionStartBatch { NumSessions = numSessions }.ToStringContent(DropboxRequestJsonContext.Default.UploadSessionStartBatch),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.BatchSessionStart);
        return content;
    }

    public async Task<Responses.BatchSessionAppend?> UploadSessionBatchAppend(byte[] bytes, Requests.UploadSessionAppendBatch appendBatches, int? delay = null)
    {
        if (delay != null)
            await Task.Delay((int)delay);

        var req = await MakePost(
            $"{_content_base_url}/files/upload_session/append_batch",
            true,
            new ByteArrayContent(bytes),
            MediaTypeNames.Application.Octet
        );
        req.Headers.Add("Dropbox-API-Arg", appendBatches.ToJson(DropboxRequestJsonContext.Default.UploadSessionAppendBatch));
        var res = await Http.client.SendAsync(req);
        if (res.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            var retryAfter = res.Headers.RetryAfter?.Delta;
            return await UploadSessionBatchAppend(bytes, appendBatches, retryAfter == null ? 1000 : (int)retryAfter.Value.TotalMilliseconds);
        }
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.BatchSessionAppend);
        return content;
    }

    public async Task<Responses.BatchSessionFinish?> UploadSessionBatchFinish(Requests.UploadSessionFinishBatch finishBatch, int? delay = null)
    {
        if (delay != null)
            await Task.Delay((int)delay);

        _logger.WriteLine($"Finishing batch");
        var req = await MakePost(
            $"{_api_base_url}/files/upload_session/finish_batch_v2",
            true,
            finishBatch.ToStringContent(DropboxRequestJsonContext.Default.UploadSessionFinishBatch),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        if (res.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            var retryAfter = res.Headers.RetryAfter?.Delta;
            return await UploadSessionBatchFinish(finishBatch, retryAfter == null ? 1000 : (int)retryAfter.Value.TotalMilliseconds);
        }
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.BatchSessionFinish);
        List<Requests.UploadSessionFinish> failedWrites = [];
        for (var i = 0; i < content?.Entries.Count; i++)
        {
            var r = content?.Entries[i];
            if (r?.Failure != null)
            {
                if (r.Failure.LookupFailed?.Tag == "too_many_write_operations")
                {
                    failedWrites.Add(finishBatch.Entries[i]);
                }
            }
        }
        if (failedWrites.Count > 0)
            return await UploadSessionBatchFinish(new() { Entries = failedWrites });
        return content;
    }

    public async Task<Stream> Download(string path)
    {
        var req = await MakePost(
            $"{_content_base_url}/files/download",
            true,
            null,
            null
        );
        req.Headers.Add("Dropbox-API-Arg", new Requests.Download { Path = path }.ToJson(DropboxRequestJsonContext.Default.Download));
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        return await res.Content.ReadAsStreamAsync();
    }

    public async Task<Responses.FileMetadata?> UploadFolderZipped(string folderPath, string savePath)
    {
        try
        {
            var baseDir = new DirectoryInfo(folderPath);
            var dirName = baseDir.Name;
            using (MemoryStream zip = new())
            {
                ZipFile.CreateFromDirectory(baseDir.FullName, zip);
                var fullSavePath = $"/{savePath}/{dirName}.zip";
                _logger.WriteLine($"Uploading:  Size  {zip.Length} Bytes \n        {folderPath} \n     -> {fullSavePath}");
                return await Upload(zip.ToArray(), fullSavePath);
            }
        }
        catch (Exception e)
        {
            _logger.WriteLine(e);
            return null;
        }
    }

    public async Task DownloadFolderZipped(string saveTo, string dropboxFolder)
    {
        try
        {
            var saveDir = new DirectoryInfo(saveTo);
            var dirName = saveDir.Name;
            _logger.WriteLine($"Download: \n        {dropboxFolder} \n     -> {saveTo}");
            var zip = await Download(dropboxFolder);
            ZipFile.ExtractToDirectory(zip, saveTo, true);
        }
        catch (Exception e)
        {
            _logger.WriteLine(e.Message);
        }
    }
}