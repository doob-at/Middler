using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using doob.Middler.Common.SharedModels.Models;

namespace doob.Middler.Core
{
    public class FileExtensionMap
    {
        private Dictionary<string, Func<FileInfo, string, MiddlerAction>> _map = new(StringComparer.CurrentCultureIgnoreCase);

        public FileExtensionMap Set(string extension, Func<FileInfo, string, MiddlerAction> handler)
        {
            _map[extension] = handler;
            return this;
        }

        internal List<string> GetRegisteredExtensions()
        {
            return _map.Keys.ToList();
        }

        internal Func<FileInfo, string, MiddlerAction>? GetFunc(string extension)
        {
            return _map.TryGetValue(extension, out var func) ? func : null;
        }
    }
}
