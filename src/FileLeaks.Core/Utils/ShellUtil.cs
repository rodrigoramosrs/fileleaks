using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.IO;
using Shell.NET;

namespace FileLeaks.Utils
{


    public class Response
    {
        public int code { get; set; }
        public string stdout { get; set; }
        public string stderr { get; set; }
    }

    public enum Output
    {
        Hidden,
        Internal,
        External
    }

    public static class Shell
    {

        public static string ExecuteShellCommand(string defaultCommand, string WinCommand = "", string LinuxCommand = "", string MacCommand = "", bool removeLineBreak = true)
        {
            string GeneralInformationResult = "";
            if (OS.IsWin())
            {
                var result = Shell.ExecuteTerminalCommand(string.IsNullOrEmpty(WinCommand) ? defaultCommand : WinCommand);
                GeneralInformationResult = !string.IsNullOrEmpty(result.stdout) ? result.stdout : result.stderr;
            }
            else if (OS.IsMac())
            {
                GeneralInformationResult = new Bash().Command(string.IsNullOrEmpty(MacCommand) ? defaultCommand : MacCommand).Output;
            }
            else if (OS.IsGnuLinux())
            {
                GeneralInformationResult = new Bash().Command(string.IsNullOrEmpty(LinuxCommand) ? defaultCommand : LinuxCommand).Output;
            }

            if (removeLineBreak)
                return GeneralInformationResult.RemoveLineBreak();
            else return GeneralInformationResult;
        }

        private static string GetFileName()
        {
            string fileName = "";
            try
            {
                switch (OS.GetCurrent())
                {
                    case "win":
                        fileName = "cmd.exe";
                        break;
                    case "mac":
                    case "gnu":
                        fileName = "/bin/bash";
                        break;
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }

            return fileName;
        }


        private static string CommandConstructor(string cmd, Output? output = Output.Hidden, string dir = "")
        {
            try
            {
                switch (OS.GetCurrent())
                {
                    case "win":
                        if (!String.IsNullOrEmpty(dir))
                        {
                            dir = $" \"{dir}\"";
                        }
                        if (output == Output.External)
                        {
                            cmd = $"{Directory.GetCurrentDirectory()}/cmd.win.bat \"{cmd.Replace("\r\n", " && ")}\"{dir}";
                        }
                        cmd = $"/c \"{cmd.Replace("\r\n", " && ")}\"";
                        break;
                    case "mac":
                    case "gnu":
                        /*if (!String.IsNullOrEmpty(dir))
                        {
                            dir = $" '{dir}'";
                        }
                        if (output == Output.External)
                        {
                            cmd = $"sh {Directory.GetCurrentDirectory()}/cmd.mac.sh '{cmd}'{dir}";
                        }*/
                        cmd = $"-c \"{cmd}\"";
                        break;
                }

            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }

            return cmd;
        }

        public static Response ExecuteTerminalCommand(string cmd, Output? output = Output.Hidden, string dir = "")
        {
            var result = new Response();
            var stderr = new StringBuilder();
            var stdout = new StringBuilder();
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = GetFileName();
                startInfo.Arguments = CommandConstructor(cmd, output, dir);
                startInfo.RedirectStandardOutput = !(output == Output.External);
                startInfo.RedirectStandardError = !(output == Output.External);
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = !(output == Output.External);
                if (!String.IsNullOrEmpty(dir) && output != Output.External)
                {
                    startInfo.WorkingDirectory = dir;
                }

                using (Process process = Process.Start(startInfo))
                {
                    switch (output)
                    {
                        case Output.Internal:
                            //$""fnewLine;

                            while (!process.StandardOutput.EndOfStream)
                            {
                                string line = process.StandardOutput.ReadLine();
                                stdout.AppendLine(line);
                                Console.WriteLine(line);
                            }

                            while (!process.StandardError.EndOfStream)
                            {
                                string line = process.StandardError.ReadLine();
                                stderr.AppendLine(line);
                                Console.WriteLine(line);
                            }
                            break;
                        case Output.Hidden:
                            stdout.AppendLine(process.StandardOutput.ReadToEnd());
                            stderr.AppendLine(process.StandardError.ReadToEnd());
                            break;
                    }
                    process.WaitForExit();
                    result.stdout = stdout.ToString();
                    result.stderr = stderr.ToString();
                    result.code = process.ExitCode;
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
            return result;
        }
    }
}
