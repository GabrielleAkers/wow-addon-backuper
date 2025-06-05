using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Threading.Tasks;
using Dropbox.Requests;

namespace Dropbox;

public class ApiClient(BearerToken token)
{
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
            new CheckUser { Query = "foo" }.ToStringContent(DropboxRequestJsonContext.Default.CheckUser),
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
            new GetAccount { AccountId = _token.AccountId! }.ToStringContent(DropboxRequestJsonContext.Default.GetAccount),
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
            new ListFolder { Path = path }.ToStringContent(DropboxRequestJsonContext.Default.ListFolder),
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
            $"{_api_base_url}/files/list_folder/co",
            true,
            new ListFolderContinue { Cursor = cursor }.ToStringContent(DropboxRequestJsonContext.Default.ListFolderContinue),
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
            new CreateFolder { Path = path }.ToStringContent(DropboxRequestJsonContext.Default.CreateFolder),
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
            new GetMetadata { Path = path }.ToStringContent(DropboxRequestJsonContext.Default.GetMetadata),
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
        req.Headers.Add("Dropbox-API-Arg", new Upload { Path = path }.ToJson(DropboxRequestJsonContext.Default.Upload));
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
        req.Headers.Add("Dropbox-API-Arg", new UploadSessionStart { Close = false }.ToJson(DropboxRequestJsonContext.Default.UploadSessionStart));
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
            new UploadSessionAppend
            {
                Cursor = new UploadSessionCursor { Offset = offset, SessionId = sessionId }
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
            new UploadSessionFinish
            {
                Cursor = new UploadSessionCursor { Offset = offset, SessionId = sessionId },
                Commit = new UploadSessionCommit { Path = path }
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
            new UploadSessionStartBatch { NumSessions = numSessions }.ToStringContent(DropboxRequestJsonContext.Default.UploadSessionStartBatch),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.BatchSessionStart);
        return content;
    }

    public async Task<Responses.BatchSessionAppend?> UploadSessionBatchAppend(byte[] bytes, UploadSessionAppendBatch appendBatches, int? delay = null)
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

    public async Task<Responses.BatchSessionFinish?> UploadSessionBatchFinish(UploadSessionFinishBatch finishBatch)
    {
        Console.WriteLine($"Finishing batch");
        var req = await MakePost(
            $"{_api_base_url}/files/upload_session/finish_batch_v2",
            true,
            finishBatch.ToStringContent(DropboxRequestJsonContext.Default.UploadSessionFinishBatch),
            MediaTypeNames.Application.Json
        );
        var res = await Http.client.SendAsync(req);
        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        var content = await res.Content.ReadFromJsonAsync(DropboxResponseJsonContext.Default.BatchSessionFinish);
        return content;
    }

    public async Task UploadFolder(string folder_path, string save_path)
    {
        var baseDir = new DirectoryInfo(folder_path);
        var dirName = baseDir.Name;
        List<string> files = [.. Directory.EnumerateFiles(folder_path, "*", SearchOption.AllDirectories)];

        List<List<string>> fileBatches = [[]];
        int fileCount = 0;
        int batchCount = 0;
        files.ForEach(f =>
        {
            if (fileCount > 1000)
            {
                fileBatches.Add([]);
                batchCount++;
                fileCount = 0;
            }
            fileBatches[batchCount].Add(f);
            fileCount++;
        });

        const long maxSize = 150 * 1024 * 1024; // 150MiB
        for (var i = 0; i < fileBatches.Count; i++)
        {
            var batchSessionIds = await UploadSessionBatchStart(fileBatches[i].Count) ?? throw new Exception("Error creating batch session ids");
            UploadSessionFinishBatch finishBatch = new() { Entries = [] };
            long bytesSent = 0;
            List<Task<Responses.BatchSessionAppend?>> appendTasks = [];
            for (var j = 0; j < fileBatches[i].Count; j++)
            {
                var f = fileBatches[i][j];
                var finfo = new FileInfo(f);
                bytesSent += finfo.Length;
                var fullNameToBase = f.Split(dirName, 2)[1];
                var fullSavePath = $"/{save_path}/{dirName}{fullNameToBase}";
                Console.WriteLine($"Uploading:  Size  {finfo.Length} \n        {f} \n     -> {fullSavePath}");
                byte[] buffer = new byte[finfo.Length];
                if (finfo.Length >= maxSize)
                {
                    throw new Exception($"File {f} too big. must use chunked upload :)");
                }
                else
                {
                    using (var fs = File.OpenRead(f))
                    {
                        await fs.ReadExactlyAsync(buffer);
                    }
                }

                var k = j;
                appendTasks.Add(Task.Run(async () => await UploadSessionBatchAppend(buffer, new()
                {
                    Entries = [ new()
                    {
                        Length = finfo.Length,
                        Close = true,
                        Cursor = new() { Offset = 0, SessionId = batchSessionIds.SessionIds[k] }
                    }]
                })));

                finishBatch.Entries.Add(
                    new()
                    {
                        Commit = new() { Path = fullSavePath },
                        Cursor = new() { Offset = finfo.Length, SessionId = batchSessionIds.SessionIds[k] }
                    }
                );
            }
            Console.WriteLine($"Bytes to send {bytesSent}");
            if (bytesSent > maxSize) throw new Exception("Upload bytes too big must chunk it :)");

            var appendResults = await Task.WhenAll(appendTasks);

            appendResults.ToList().ForEach(r => r?.Entries.ForEach(e =>
            {
                if (e.Failure != null)
                {
                    Console.WriteLine($"Append result: {e.Failure}");
                }
            }));

            var finishRes = await UploadSessionBatchFinish(finishBatch);
            finishRes?.Entries.ForEach(e =>
            {
                if (e.Failure != null)
                {
                    Console.WriteLine($"finish fail {e.Failure}");
                }
            });
        }
    }
}