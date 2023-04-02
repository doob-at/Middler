using System.Collections.Generic;

namespace doob.Middler.Common
{
    public interface IMiddlerResponseContext
    {
        int StatusCode { get; set; }
        Dictionary<string, string> Headers { get; set; }

        void SetBody(object? body);
    }
}
