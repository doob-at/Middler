using Microsoft.AspNetCore.StaticFiles;

namespace doob.Middler.Action.Scripting.Helper
{
    public class MimeTypeMappingService
    {
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new ();

        public string GetMimeType(string fileName)
        {

            return _contentTypeProvider.TryGetContentType(fileName, out var mimeType)
                ? mimeType
                : "application/octet-stream";
        }

        public void AddMapping(string extension, string mimeType)
        {
            _contentTypeProvider.Mappings.Add(extension, mimeType);
        }
    }
}
