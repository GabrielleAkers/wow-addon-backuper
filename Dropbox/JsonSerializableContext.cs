using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dropbox;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AuthTokenResponse))]
[JsonSerializable(typeof(RefreshTokenResponse))]
internal partial class TokenResponseJsonContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(IDictionary<string, string>), TypeInfoPropertyName = "KeyValueDict")]
internal partial class KeyValueJsonContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(UserAccountInfo))]
internal partial class ApiDataJsonContext : JsonSerializerContext
{
}