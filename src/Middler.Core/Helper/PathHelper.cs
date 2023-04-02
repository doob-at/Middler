using System.IO;

namespace doob.Middler.Core.Helper
{
    public class PathHelper
    {
        public static string GetFullPath(string path, string contentPath)
        {
            var p = Path.GetFullPath(Path.Combine(contentPath, path));
            return p.Replace("\\", "/");
        }
    }
}
