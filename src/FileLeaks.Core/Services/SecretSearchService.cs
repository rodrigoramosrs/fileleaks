using FileLeaks.Core.Events;
using FileLeaks.Core.Models;
using FileLeaks.Core.Models.Events;
using FileLeaks.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileLeaks.Core.Services
{
    public class SecretSearchService : ScanEvents
    {

        //public delegate void ReportTotalFiles();
        //public event ReportTotalFiles On_ReportTotalFilesToSearch;

        private readonly FileService _FileService;
        private readonly RegexService _RegexService;
        private readonly int _MaxFileSizeMB;

        public SecretSearchService(string RegexDirectory, int MaxFileSizeMB = 50)
        {
            _FileService = new FileService();
            _RegexService = new RegexService(RegexDirectory);
            _MaxFileSizeMB = MaxFileSizeMB;
        }

        public IEnumerable<SecretResult> SearchSecretInFolder(string directory)
        {
            var result = new List<SecretResult>();
            var Files = _FileService.DirectorySearch(directory).ToList();
            this.NotifyStart();
            this.NotifyTotalFilesToSearch(Files.Count);

            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                int percetage = 100 * (i + 1) / Files.Count;
                this.NotifyProgressChange(new FileScanProgress() { Filename = file, ProgressPercentage = percetage });

                var secretFindResult = FindSecretInFile(file);

                if (secretFindResult == null) continue;

                result.Add(new SecretResult()
                {
                    FilePath = file,
                    MatchResultList = secretFindResult,
                    //Content = File.ReadAllText(file)
                });

            }

            this.NotifyFinish(result);
            return result;
        }



        public IEnumerable<MatchResult> FindSecretInFile(string filePath)
        {
            if (SizeUtils.ConvertBytesToMegabytes(new FileInfo(filePath).Length) > _MaxFileSizeMB) return null;

            var result = new List<MatchResult>();
            var FileContent = File.ReadAllLines(filePath);
            var dictionaryWithMatch = _RegexService.IsMatch(FileContent);

            if (dictionaryWithMatch.Count <= 0) return null;

            
            foreach (var itemWithMach in dictionaryWithMatch)
            {
                var matchResult = new MatchResult()
                {
                    Name = itemWithMach.Key,
                    Index = itemWithMach.Value.Index,
                    Length = itemWithMach.Value.Length,
                    Result = itemWithMach.Value.Value,
                };
                result.Add(matchResult);
                this.NotifyOnSecretFound(matchResult);
            }
            return result;
        }



    }
}
