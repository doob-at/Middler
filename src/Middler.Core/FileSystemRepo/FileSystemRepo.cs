using System;
using System.Collections.Generic;
using System.IO;
using doob.Middler.Common.Interfaces;
using doob.Middler.Common.SharedModels.Models;
using doob.Reflectensions.Common;

namespace doob.Middler.Core
{
    public class FileSystemRepo : IMiddlerRepository
    {

        public DirectoryInfo FSRoot { get; }

        public string? Prefix { get; }

        public FileExtensionMap FileExtensionMap { get; }

        public FileSystemRepo(string path, Action<FileExtensionMap> fileMapping)
        {
            FSRoot =  new DirectoryInfo(path);
            if (!FSRoot.Exists)
            {
                throw new DirectoryNotFoundException(FSRoot.FullName);
            }

            FileExtensionMap = fileMapping.InvokeAction();
        }

        public FileSystemRepo(string path, Action<FileExtensionMap> fileMapping, string prefix): this(path, fileMapping)
        {
            Prefix = prefix;
        }

        public List<MiddlerRule> ProvideRules()
        {
            var files = FSRoot.GetFiles("*.*", SearchOption.AllDirectories );
            var extensions = FileExtensionMap.GetRegisteredExtensions();
            var list = new List<MiddlerRule>();
            foreach (var fileInfo in files)
            {
                var ext = fileInfo.Extension?.Trim(".");
                if(String.IsNullOrWhiteSpace(ext))
                    continue;

                if (extensions.Contains(ext))
                {
                    var func = FileExtensionMap.GetFunc(ext);
                    
                    var action = func?.Invoke(fileInfo, File.ReadAllText(fileInfo.FullName));
                    if (action != null)
                    {
                        var rule = new MiddlerRule();
                        rule.Path = fileInfo.FullName.Substring(FSRoot.FullName.Length).Replace("\\", "/");
                        rule.Name = rule.Path;
                        rule.Actions = new() { action };
                        list.Add(rule);
                    }
                    
                }
            }

            return list;
        }
    }

    
}
