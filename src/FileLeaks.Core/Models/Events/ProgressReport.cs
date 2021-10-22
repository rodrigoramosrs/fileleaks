using System;
using System.Collections.Generic;
using System.Text;

namespace FileLeaks.Core.Models.Events
{
    public class FileScanProgress : EventArgs
    {
        public int ProgressPercentage { get; set; }
        public string Filename { get; set; }
    }
}
