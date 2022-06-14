using FileLeaks.Core.Events;
using FileLeaks.Core.Models;
using FileLeaks.Core.Models.Events;
using FileLeaks.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly string[] _ExtensionsToIgnore;

        public SecretSearchService(int MaxFileSizeMB = 200)
        {
            _FileService = new FileService();
            _RegexService = new RegexService();
            _MaxFileSizeMB = MaxFileSizeMB;

            string Filename = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "extensions_to_ignore.conf");

            if (File.Exists(Filename))
                _ExtensionsToIgnore = File.ReadAllLines(Filename);
            else
                _ExtensionsToIgnore = new string[0];
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

                if (_ExtensionsToIgnore.Contains(Path.GetExtension(file).ToLower().Replace(".", ""))) continue;

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
            result?.OrderBy(x => x.FilePath)?.ToList()?.ForEach(x => x.MatchResultList.OrderBy(y => y.Name));
            return result; //.ThenBy(x => x.MatchResultList.OrderBy(y => y.Name));
        }



        public IEnumerable<MatchResult> FindSecretInFile(string filePath)
        {
            if (SizeUtils.ConvertBytesToMegabytes(new FileInfo(filePath).Length) > _MaxFileSizeMB) return null;

            bool UseLegacy = true;
            IEnumerable<MatchResult> result = new List<MatchResult>();
            if (UseLegacy)
            {
                var FileContent = GetFileContent(filePath);
                //var FileContent = File.ReadAllLines(filePath);
                //
                result = _RegexService.IsMatch(FileContent);
            }
            else
            {
                _RegexService.IsMatchFromFile(filePath);
            }

            if (result?.Count() > 0)
            {
                foreach (var secret in result)
                {
                    this.NotifyOnSecretFound(secret);
                }
            }

            return result;
        }

        private string[] GetFileContent(string filePath)
        {
            FileInfo info = new FileInfo(filePath);
            string[] result = null;
            string FileContent = File.ReadAllText(filePath);
            try
            {
                switch (info.Extension)
                {
                    case ".js":

                        result = new Jsbeautifier.Beautifier(new Jsbeautifier.BeautifierOptions() { })
                            .Beautify(FileContent)
                            .Split('\n');
                        break;
                    case ".json":
                        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(FileContent);
                        result = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented).Split('\n');
                        break;
                    default:
                        //result = File.ReadAllLines(filePath);
                        result = FileContent.Split('\n');
                        break;
                }
            }
            catch (Exception)
            {
                //TODO: IMPLEMENT EXCEPTION
                result = FileContent.Split('\n');
            }


            return result ?? new string[] { "" };
        }
    }
}
