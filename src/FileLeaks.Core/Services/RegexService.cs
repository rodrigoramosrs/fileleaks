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

        private readonly Dictionary<string, Regex> _CompiledRegexDictionary;
        private readonly Dictionary<string, string> _SimpleRegexDictionary;

        public static string RegexJsonContent { get; set; }
        public RegexService()
        {
            _CompiledRegexDictionary = new Dictionary<string, Regex>();
            _SimpleRegexDictionary = new Dictionary<string, string>();

            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(RegexJsonContent);

            foreach (var item in jsonData)
            {
                _CompiledRegexDictionary.Add(item.Key, new Regex(item.Value, RegexOptions.Compiled, TimeSpan.FromSeconds(60)));
                _SimpleRegexDictionary.Add(item.Key, item.Value);
            }

        }

        public IEnumerable<MatchResult> IsMatchFromFile(string File)
        {
            var result = new List<MatchResult>();
            foreach (var regex in _SimpleRegexDictionary)
            {
                var Match = RipGrepService.RegexSearch(File, regex.Value);

                result.Add(new MatchResult()
                {
                    Name = regex.Key,
                    Index = Match.Index,
                    Length = Match.Result.Length,
                    Result = Match.Result,
                    Content = "",
                });
            }
            return result;


        }

        public IEnumerable<MatchResult> IsMatch(string[] Content)
        {
            //Dictionary<string, Match> result = new Dictionary<string, Match>();
            List<MatchResult> result = new List<MatchResult>();

            string FullContent = string.Join("\n", Content);
            var foreachResult = Parallel.ForEach(_CompiledRegexDictionary,
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
                            var Match = keyValuePair.Value.Match(FullContent);// Regex.Match(line, keyValuePair.Value, RegexOptions.None, TimeSpan.FromSeconds(60));
                            List<MatchResult> localResult = new List<MatchResult>();
                            if (Match.Success)
                            {
                                do
                                {
                                    localResult.Add(new MatchResult()
                                    {
                                        Name = keyValuePair.Key,
                                        Index = Match.Index,
                                        Length = Match.Value.Length,
                                        Result = Match.Value,
                                        Content = Match.Value,
                                    });
                                    Match = Match.NextMatch();

                                } while (Match.Success);

                                lock (result)
                                {
                                    result.AddRange(localResult);
                                    tokenSource.Cancel();
                                }

                            }

                            tokenSource.Cancel();


                        }, token);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {

                        var exception = ex;
                    }

                });

            while (!foreachResult.IsCompleted)
                Thread.Sleep(1000);

            return result.Count <= 0 ? null : result;
        }
    }
}
