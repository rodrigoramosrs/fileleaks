using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GitOwner
{
    public static class GlobalConfiguration
    {
        static GlobalConfiguration()
        {
            SetWorkPath(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
        }
        public static string WorkPath { get; private set; }

        public static string OutputPath
        {
            get { return $"{GlobalConfiguration.WorkPath}/fileleaks_output"; }
        }

        public static void SetWorkPath(string workPath)
        {
            WorkPath = workPath;
        }
    }
}
