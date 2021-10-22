using System;
using System.Collections.Generic;
using System.Text;

namespace FileLeaks.Utils
{
    public static class SizeUtils
    {
        public static string ConvertFileSizeToReadable(this long Length)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
           
            int order = 0;
            while (Length >= 1024 && order < sizes.Length - 1)
            {
                order++;
                Length = Length / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", Length, sizes[order]);
        }

        public static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public static double ConvertKilobytesToMegabytes(long kilobytes)
        {
            return kilobytes / 1024f;
        }
    }
}
