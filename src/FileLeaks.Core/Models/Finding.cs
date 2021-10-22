using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileLeaks.Core.Models
{
    public class Finding
    {
        public string FileDir { get; set; }
        public string Secret { get; set; }
    }
}
