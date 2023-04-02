using System;
using System.IO;

namespace doob.Middler.Common.StreamHelper
{
    public record AutoStreamOptions(int MemoryThreshold, string TempDirectory, string? FilePrefix);

    public class AutoStreamOptionsBuilder
    {
        private AutoStreamOptions _autoStreamOptions = new(32 * 1024, Directory.GetCurrentDirectory(), "");

        public AutoStreamOptionsBuilder WithFilePrefix(string value)
        {
            _autoStreamOptions = _autoStreamOptions with {FilePrefix = value};
            return this;
        }
        public AutoStreamOptionsBuilder WithTempDirectory(string value)
        {
            _autoStreamOptions = _autoStreamOptions with { TempDirectory = value };
            return this;
        }

        public AutoStreamOptionsBuilder WithMemoryThreshold(int value)
        {
            _autoStreamOptions = _autoStreamOptions with { MemoryThreshold = value };
            return this;
        }

        public static implicit operator AutoStreamOptions(AutoStreamOptionsBuilder builder)
        {
            return builder._autoStreamOptions;
        }

        public static implicit operator AutoStreamOptionsBuilder(Action<AutoStreamOptionsBuilder> options)
        {
            var optsBuilder = new AutoStreamOptionsBuilder();
            options?.Invoke(optsBuilder);
            return optsBuilder;
        }
    }
}
