using FileLeaks.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileLeaks.Core.Services
{
    public class RegexService
    {
        private readonly Dictionary<string, string> _RegexDictionary;
        public RegexService(string RegexDirectory)
        {
            _RegexDictionary = new Dictionary<string, string>();

            foreach (string filePath in Directory.GetFiles(RegexDirectory, "*.json", SearchOption.AllDirectories))
            {
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath));
                foreach (var item in jsonData)
                {
                    _RegexDictionary.Add(item.Key, item.Value);
                }
            }

        }

        public IEnumerable<MatchResult> IsMatch(string[] Content)
        {
            //Dictionary<string, Match> result = new Dictionary<string, Match>();
            List<MatchResult> result = new List<MatchResult>();
            foreach (var line in Content)
            {
                Parallel.ForEach(_RegexDictionary,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 10
                },
                keyValuePair =>
                {

                    var Match = Regex.Match(line, keyValuePair.Value);
                    if (Match.Success)
                    {
                        lock (result)
                        {
                            var matchResult = new MatchResult()
                            {
                                Name = keyValuePair.Key,
                                Index = Match.Index,
                                Length = Match.Value.Length,
                                Result = Match.Value,
                                Content = line,
                            };
                            result.Add(matchResult);
                        }

                    }
                });
            }

            return result.Count <= 0 ? null : result;
        }
    }
}
