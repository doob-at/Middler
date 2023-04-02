using System;
using doob.Middler.Core.JsonConverters;
using doob.Reflectensions;

namespace doob.Middler.Core
{
    public class Converter
    {
        private static readonly Lazy<Json> lazyJson = new Lazy<Json>(() => new Json()
            .RegisterJsonConverter<DecimalJsonConverter>()
        );

        public static Json Json => lazyJson.Value;

    }
}
