using FileLeaks.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FileLeaks.Core.Services
{
    public class RegexService
    {
        private static TimeSpan ExecutionTimeout = TimeSpan.FromMinutes(1);

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


            Parallel.ForEach(Content,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 10
                },
                line =>
                {
                    if (string.IsNullOrEmpty(line)) return;

                    Parallel.ForEach(_RegexDictionary,
                        new ParallelOptions
                        {
                            MaxDegreeOfParallelism = Environment.ProcessorCount * 10,

                        },
                        keyValuePair =>
                        {

                            //Console.WriteLine(line);


                            try
                            {
                                var tokenSource = new CancellationTokenSource(ExecutionTimeout);
                                var token = tokenSource.Token;

                                Task task = Task.Factory.StartNew(() =>
                                {
                                    var Match = Regex.Match(line, keyValuePair.Value);
                                    if (Match.Success)
                                    {
                                        lock (result)
                                        {
                                            result.Add(new MatchResult()
                                            {
                                                Name = keyValuePair.Key,
                                                Index = Match.Index,
                                                Length = Match.Value.Length,
                                                Result = Match.Value,
                                                Content = line,
                                            });
                                            tokenSource.Cancel();
                                        }


                                    }
                                }, token);
                                task.Wait();
                            }
                            catch (Exception ex)
                            {

                                var exception = ex;
                            }

                        });
                });

            //foreach (var line in Content)
            //{
            //    Parallel.ForEach(_RegexDictionary,
            //    new ParallelOptions
            //    {
            //        MaxDegreeOfParallelism = Environment.ProcessorCount * 10
            //    },
            //    keyValuePair =>
            //    {

            //        var Match = Regex.Match(line, keyValuePair.Value);
            //        if (Match.Success)
            //        {
            //            var matchResult = new MatchResult()
            //            {
            //                Name = keyValuePair.Key,
            //                Index = Match.Index,
            //                Length = Match.Value.Length,
            //                Result = Match.Value,
            //                Content = line,
            //            };

            //            lock (result)
            //            {
            //                result.Add(matchResult);
            //            }

            //        }
            //    });
            //}

            return result.Count <= 0 ? null : result;
        }
    }
}
