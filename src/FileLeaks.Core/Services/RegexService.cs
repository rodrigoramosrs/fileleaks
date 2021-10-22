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

        public Dictionary<string, Match> IsMatch(string[] Content)
        {
            Dictionary<string, Match> result = new Dictionary<string, Match>();

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

                            if (result.ContainsKey(keyValuePair.Key))
                            {
                                int count = 0;
                                string keyName;
                                do
                                {
                                    count++;
                                    keyName = $"{keyValuePair.Key}_{count}";
                                }
                                while (result.ContainsKey(keyName));
                                result.Add(keyName, Match);
                            }
                            else
                                result.Add(keyValuePair.Key, Match);
                        }

                    }
                });
            }



            //foreach (var regex in _RegexDictionary)
            //{
            //    //var matchCollection = Regex.Matches(Content, regex.Value);
            //    //if (matchCollection.Count <= 0) continue;
            //    var Match = Regex.Match(Content, regex.Value);
            //    if (!Match.Success) continue;

            //    result.Add(regex.Key, Match);

            //    //result = result || Regex.IsMatch(Content, regex.Value);
            //    //return true;
            //}


            return result;
        }
    }
}
