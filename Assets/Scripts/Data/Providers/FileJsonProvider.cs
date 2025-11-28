using System.IO;

namespace Data
{
    /// <summary>
    /// Lê JSON de database
    /// </summary>
    public class FileJsonProvider : IJsonProvider
    {
        private readonly string _rootPath;

        public FileJsonProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        public string LoadText(string relativePath)
        {
            var fullPath = Path.Combine(_rootPath, relativePath);
            return File.ReadAllText(fullPath);
        }
    }
}
