using System;
using System.Collections.Generic;

namespace doob.Middler.Common
{
    public interface IMiddlerContext
    {
        IMiddlerRequestContext Request { get; }

        IMiddlerResponseContext Response { get; }

        Dictionary<string, object> PropertyBag { get; }

        IServiceProvider RequestServices { get; }
    }
}
