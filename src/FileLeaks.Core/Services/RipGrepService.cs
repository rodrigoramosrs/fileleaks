using FileLeaks.Core.Models;
using FileLeaks.Utils;
using GitOwner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileLeaks.Core.Services
{
    public static class RipGrepService
    {
        private static double ExecutionTimeout = TimeSpan.FromMinutes(30).TotalMilliseconds;
        private static string GetAssemblyName()
        {
            Dictionary<string, string> dictionary = new System.Collections.Generic.Dictionary<string, string>();

            dictionary.Add("winx86", "rg.exe");
            dictionary.Add("winx64", "rg.exe");
            dictionary.Add("winarm", "rg.exe");
            dictionary.Add("winarm64", "rg.exe");

            dictionary.Add("macx86", "rg");
            dictionary.Add("macx64", "rg");
            dictionary.Add("macarm", "rg");
            dictionary.Add("macarm64", "rg");

            dictionary.Add("linuxx86", "rg");
            dictionary.Add("linuxx64", "rg");
            dictionary.Add("linuxarm", "rg");
            dictionary.Add("linuxarm64", "rg");


            var OsName = OS.GetCurrent();
            string architecture = (string)RuntimeInformation.OSArchitecture.ToString().ToLower();

            return dictionary[$"{OsName}{architecture}"];

        }
        public static MatchResult RegexSearch(string Path, string Regex)
        {
            try
            {
                MatchResult result = new MatchResult();
                string AssemblyName = GetAssemblyName();
                StringBuilder parameters = new StringBuilder();
                parameters.Append($" \"{Regex}\" ");

                parameters.Append($" \"{Path}\" ");

                string Command = $"{GlobalConfiguration.WorkPath}/External/Tools/RipGrep/{AssemblyName} {parameters.ToString()}";

                string output = ExexuteTask(Command);
                result.Result = output;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
                WriteLineConsole(ex.ToString());
            }

        }

        private static string ExexuteTask(string Command)
        {
            string output = string.Empty;
            CancellationTokenSource ts = new CancellationTokenSource();
            Task toolTask = Task.Run(() =>
            {
                WriteLineConsole($"Starting RipGrep - Current timeout: { (ExecutionTimeout / 1000) / 60} minutes");
                KillLostToolProcess();
                output = Utils.Shell.ExecuteShellCommand(Command);
                WriteLineConsole(" |- Finished RipGrep");
            });

            try
            {
                bool result = toolTask.Wait((int)ExecutionTimeout, ts.Token);
                if (!result)
                {
                    WriteLineConsole($" |- Timeout: current status {toolTask.Status}. Shutdown process...");
                    KillLostToolProcess();
                }
                ts.Dispose();
            }
            catch (OperationCanceledException e)
            {
                WriteLineConsole(string.Format(" |- {0}: The wait has been canceled. Task status: {1:G}",
                                  e.GetType().Name, toolTask.Status));
                KillLostToolProcess();
                ts.Dispose();
            }

            return output;
        }

        private static void WriteLineConsole(string message)
        {

            Console.WriteLine(message);
        }

        private static string KillLostToolProcess()
        {
            string LinuxCommand = $"pkill {GetAssemblyName()}";
            string WindowsCommand = $"taskkill /im {GetAssemblyName()} -f";
            string result = Utils.Shell.ExecuteShellCommand(LinuxCommand, WinCommand: WindowsCommand);
            return result;
        }
    }
}
