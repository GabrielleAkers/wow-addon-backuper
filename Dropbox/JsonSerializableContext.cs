using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dropbox;

[JsonSerializable(typeof(Requests.CheckUser))]
[JsonSerializable(typeof(Requests.GetAccount))]
[JsonSerializable(typeof(Requests.ListFolder))]
[JsonSerializable(typeof(Requests.ListFolderContinue))]
[JsonSerializable(typeof(Requests.CreateFolder))]
[JsonSerializable(typeof(Requests.GetMetadata))]
[JsonSerializable(typeof(Requests.UploadSessionStart))]
[JsonSerializable(typeof(Requests.UploadSessionAppend))]
[JsonSerializable(typeof(Requests.UploadSessionFinish))]
[JsonSerializable(typeof(Requests.Upload))]
[JsonSerializable(typeof(Requests.UploadSessionStartBatch))]
[JsonSerializable(typeof(Requests.UploadSessionAppendBatch))]
[JsonSerializable(typeof(Requests.UploadSessionFinishBatch))]
[JsonSerializable(typeof(Requests.Download))]
internal partial class DropboxRequestJsonContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Responses.AuthToken))]
[JsonSerializable(typeof(Responses.RefreshToken))]
[JsonSerializable(typeof(Responses.UserAccountInfo))]
[JsonSerializable(typeof(Responses.ListFolder))]
[JsonSerializable(typeof(Responses.CreateFolder))]
[JsonSerializable(typeof(Responses.FileMetadata))]
[JsonSerializable(typeof(Responses.SessionStart))]
[JsonSerializable(typeof(Responses.BatchSessionStart))]
[JsonSerializable(typeof(Responses.BatchSessionAppend))]
[JsonSerializable(typeof(Responses.BatchSessionFinish))]
internal partial class DropboxResponseJsonContext : JsonSerializerContext
{
}



[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(IDictionary<string, string>), TypeInfoPropertyName = "KeyValueDict")]
internal partial class KeyValueJsonContext : JsonSerializerContext
{
}