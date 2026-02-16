using System.Collections.Generic;
using System.Linq;

namespace KSeF.Client.Core.Infrastructure.Rest
{
    public sealed class RestResponse<T>
    {
        public T Body { get; }
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

        public RestResponse(T body, IReadOnlyDictionary<string, IEnumerable<string>> headers)
        {
            Headers = headers ?? new Dictionary<string, IEnumerable<string>>();
            Body = body;
        }

        public bool TryGetHeader(string name, out IEnumerable<string> values) =>
            Headers.TryGetValue(name, out values);

        public bool TryGetHeaderSingle(string name, out string value)
        {
            if (Headers.TryGetValue(name, out IEnumerable<string> values))
            {
                value = values.FirstOrDefault();
                return !string.IsNullOrEmpty(value);
            }

            value = null;
            return false;
        }
    }
}
