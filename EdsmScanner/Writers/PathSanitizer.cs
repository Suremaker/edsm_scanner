using System;
using System.IO;

namespace EdsmScanner.Writers
{
    internal static class PathSanitizer
    {
        public static string SanitizePath(string fileName)
        {
            var invalids = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
    }
}