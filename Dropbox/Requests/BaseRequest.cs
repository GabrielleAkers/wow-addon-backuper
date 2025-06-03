using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Dropbox.Requests;

record class BaseRequest
{
    public virtual string ToJson<T>(JsonTypeInfo<T> json_type_info)
    {
        return JsonSerializer.Serialize(this, json_type_info);
    }

    public virtual StringContent ToHttpContent<T>(JsonTypeInfo<T> json_type_info)
    {
        return new StringContent(ToJson(json_type_info));
    }
}
