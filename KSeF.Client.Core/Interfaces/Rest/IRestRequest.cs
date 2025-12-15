using System;
using System.Collections.Generic;
using System.Net.Http;

namespace KSeF.Client.Core.Interfaces.Rest
{
    public interface IRestRequest
    {
        string Path { get; }
        HttpMethod Method { get; }
        string AccessToken { get; }
        string ContentType { get; }   
        string Accept { get; }   
        IDictionary<string, string> Headers { get; }
        IDictionary<string, string> Query { get; }
        TimeSpan? Timeout { get; }
        string ApiVersion { get; }
    }
}
