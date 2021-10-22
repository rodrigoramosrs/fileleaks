using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileLeaks.Core.Models
{
    public class SecretResult
    {
        public SecretResult()
        {
            MatchResultList = new List<MatchResult>();
        }
        public string FilePath { get; set; }
        //public string Content { get; set; }
        public IEnumerable<MatchResult> MatchResultList { get; set; }
    }

    public class MatchResult
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }
        public string Result { get; set; }
        public string Content { get; internal set; }
    }
}
