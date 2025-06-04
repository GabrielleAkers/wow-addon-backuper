using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dropbox;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Responses.AuthToken))]
[JsonSerializable(typeof(Responses.RefreshToken))]
[JsonSerializable(typeof(Responses.UserAccountInfo))]
[JsonSerializable(typeof(Responses.ListFolder))]
[JsonSerializable(typeof(Responses.CreateFolder))]
[JsonSerializable(typeof(Responses.GetMetadata))]
internal partial class DropboxResponseJsonContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Requests.CheckUser))]
[JsonSerializable(typeof(Requests.GetAccount))]
[JsonSerializable(typeof(Requests.ListFolder))]
[JsonSerializable(typeof(Requests.CreateFolder))]
[JsonSerializable(typeof(Requests.GetMetadata))]
internal partial class DropboxRequestJsonContext : JsonSerializerContext
{

}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(IDictionary<string, string>), TypeInfoPropertyName = "KeyValueDict")]
internal partial class KeyValueJsonContext : JsonSerializerContext
{
}