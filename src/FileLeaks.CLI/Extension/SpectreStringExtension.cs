using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileLeaks.Extension
{
    public static class SpectreStringExtension
    {
        public static string NormalizeString(this string value)
        {
            return value
                .Replace("'", "''")
                .Replace("\"", "\"\"")
                .Replace("[", "[[")
                .Replace("]", "]]")
                .Replace("/", "//")
                .Replace("\\", "\\\\");
        }
    }
}
