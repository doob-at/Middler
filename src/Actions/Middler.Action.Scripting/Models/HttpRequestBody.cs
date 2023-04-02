using System;
using System.IO;
using doob.Middler.Action.Scripting.Helper;
using doob.Middler.Common.Interfaces;
using doob.Middler.Core.Helper;
using doob.Scripter.Shared;
using Microsoft.AspNetCore.Http;

namespace doob.Middler.Action.Scripting.Models
{
    public class HttpRequestBody {

        private readonly IScriptEngine _scriptEngine;
        private readonly IMiddlerOptions _middlerOptions;
        private HttpRequest _httpRequest;

        private string? _text;
        private object? _json;
        private SimpleXml? _xml;


        public HttpRequestBody(HttpRequest httpRequest, IScriptEngine scriptEngine, IMiddlerOptions middlerOptions) {
            _httpRequest = httpRequest;
            _scriptEngine = scriptEngine;
            _middlerOptions = middlerOptions;
        }

        public Stream Stream()
        {
            return _httpRequest.Body;
        }

        public string? Text() {
            if (!IsEmpty && !MultipartRequestHelper.IsMultipartContentType(_httpRequest.ContentType))
                return _text ??= ReadAsString(_httpRequest.Body);

            return null;
        }

        public object? Json() {
            if (!IsEmpty && !MultipartRequestHelper.IsMultipartContentType(_httpRequest.ContentType))
                return _json ??= _scriptEngine.JsonParse(Text());

            return _scriptEngine.JsonParse("null");
        }

        public SimpleXml Xml() {
            if(!IsEmpty && !MultipartRequestHelper.IsMultipartContentType(_httpRequest.ContentType))
                return _xml ??= new SimpleXml(Text());

            return new SimpleXml();
        }

        public MultiPartContent SaveFiles(string? path = null) {

            if (IsEmpty || !MultipartRequestHelper.IsMultipartContentType(_httpRequest.ContentType)) {
                return null;
            }

            if (String.IsNullOrWhiteSpace(path))
            {
                path = _middlerOptions.TemporaryFilePath;
            }

            path = PathHelper.GetFullPath(path, "");
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                Directory.CreateDirectory(dirInfo.FullName);
            }

            MultiPartContent files;

           files = _httpRequest.StreamFiles(dirInfo).GetAwaiter().GetResult();
            
            return files;
        }

        public bool IsEmpty => _httpRequest.Body == null || _httpRequest.ContentLength < 1;


        private string ReadAsString(Stream stream) {

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
       
    }
}
