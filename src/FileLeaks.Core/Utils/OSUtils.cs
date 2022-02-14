using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FileLeaks.Utils
{
    public static class OS
    {
        public static bool IsWin() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMac() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsGnuLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static string GetCurrent()
        {
            string result = 
            (IsWin() ? "win" : null) ??
            (IsMac() ? "mac" : null) ??
            (IsGnuLinux() ? "linux" : null);

            return result;
        }

        public static OSPlatform GetCurrentEnum()
        {

            if (IsWin())
                return OSPlatform.Windows;
            else if (IsMac())
                return OSPlatform.OSX;
            else
                return OSPlatform.Linux;
        }


    }
}
