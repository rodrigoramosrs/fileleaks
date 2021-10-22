using FileLeaks.CLI.Command;
using FileLeaks.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.IO;
using System.Reflection;

namespace FileLeaks
{
    class Program
    {

        static void Main(string[] args)
        {
            ExtractEmbededResources();

            var app = new CommandApp<SearchSecretCommand>();
            app.Configure(config =>
            {
                config.SetApplicationName("FileLeaks");
            });
            PrintHeader();
            app.Run(args);

        }

        private static void PrintHeader()
        {
            AnsiConsole.Write(
            new FigletText("FileLeaks")
                .LeftAligned()
                .Color(Color.Green));

            AnsiConsole.MarkupLine($"[bold yellow]Scan files for secrets just easy![/]");
            AnsiConsole.MarkupLine("[bold gray]repo: https://github.com/rodrigoramosrs/fileleaks[/]");
        }

        private static void ExtractEmbededResources()
        {

            string resourceName = "FileLeaks.regex.all.json";
            string outputName = "all.json";
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            if (!Directory.Exists(@"./regex"))
                Directory.CreateDirectory(@"./regex");

            FileStream outputFileStream = new FileStream(@$"./regex/{outputName}", FileMode.OpenOrCreate);
            Stream res = currentAssembly.GetManifestResourceStream(resourceName);
            if (res != null)
            {
                res.CopyTo(outputFileStream);
                res.Close();
            }
            outputFileStream.Close();
        }
    }
}
