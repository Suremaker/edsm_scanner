using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EdsmScanner.Writers;

namespace EdsmScanner.Clients
{
    internal class SystemCache
    {
        private readonly string _path = Path.Combine(AppContext.BaseDirectory, "_system-cache");
        private readonly TimeSpan _expirationWindow = TimeSpan.FromMinutes(30);

        public SystemCache()
        {
            Directory.CreateDirectory(_path);
        }

        public string? TryRetrieve(string systemName)
        {
            var file = GetFullPath(systemName);

            if (!File.Exists(file))
                return null;

            var content = File.ReadAllText(file);

            var lineBreak = content.IndexOf('\n', StringComparison.Ordinal);
            if (lineBreak <= 0
                || !long.TryParse(content.AsSpan(0, lineBreak), out var timeInt)
                || DateTimeOffset.FromUnixTimeSeconds(timeInt) < DateTimeOffset.UtcNow - _expirationWindow)
                return null;

            return content.Substring(lineBreak + 1);
        }

        public string Update(string systemName, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return body;

            var contents = new StringBuilder()
                .Append(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                .Append('\n')
                .Append(body)
                .ToString();

            File.WriteAllText(GetFullPath(systemName), contents);
            return body;
        }

        private string GetFullPath(string systemName) => Path.Combine(_path, PathSanitizer.SanitizePath(systemName));
    }
}