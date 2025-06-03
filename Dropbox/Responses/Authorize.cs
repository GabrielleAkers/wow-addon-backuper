using System;
using System.Web;

namespace Dropbox.Responses;

class Authorize
{
    public bool IsError { get; private set; }
    public string? Error { get; private set; }
    public string? ErrorDescription { get; private set; }
    public string? Code { get; private set; }
    public string? State { get; private set; }

    public static Authorize ParseRequestURL(string? raw_request_url, string original_state)
    {
        var oauth_state = new Authorize();
        if (!string.IsNullOrEmpty(raw_request_url))
        {
            Console.WriteLine($"Raw url {raw_request_url} and state {original_state}");
            var parsed = raw_request_url.Split("?")[1];
            var query_params = HttpUtility.ParseQueryString(parsed);

            oauth_state.Error = query_params.Get("error");
            oauth_state.IsError = !string.IsNullOrEmpty(oauth_state.Error);
            oauth_state.ErrorDescription = query_params.Get("error_description");
            oauth_state.Code = query_params.Get("code");
            oauth_state.State = query_params.Get("state");
        }
        return oauth_state;
    }
}