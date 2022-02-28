using FileLeaks.Core;
using FileLeaks.Extension;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileLeaks.CLI.Command
{
    public class SearchSecretCommand : Command<SearchSecretCommand.Settings>
    {
        private readonly IAnsiConsole _console;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[EXAMPLE]")]
            [Description("The example to run.\nIf none is specified, all examples will be listed")]
            public string Name { get; set; }

            [CommandOption("-p|--path")]
            [Description("Path to scan")]
            public string Path { get; set; }

        }


        public SearchSecretCommand(IAnsiConsole console)
        {
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            Boot(settings).GetAwaiter().GetResult();
            return 0;
        }


        private static bool ScanFinished = false;
        private static ProgressTask CurrentProgressTask;
        private static ProgressTask CurrentFileTask;
        private static ProgressTask CurrentSecretsTask;
        private static ProgressContext CurrentContext;

        private async Task Boot(Settings settings)
        {
            _console.WriteLine();
            if (string.IsNullOrEmpty(settings.Path))
            {
                _console.MarkupLine("[bold red]paramter path is empty. Please use the parameter \"-p {path}\" [/]");
                _console.MarkupLine("[bold gray]eg: Fileleaks -p ./path/to/search [/]");
                return;
            }

            _console.MarkupLine($"[bold yellow]Starting scanner...[/]");
            _console.MarkupLine($"[bold yellow]Path: {settings.Path}[/]");

            var fileLeakerCore = new FileLeakCore();

            fileLeakerCore.OnFinished += FileLeakerCore_OnFinished;
            fileLeakerCore.OnProgressChange += FileLeakerCore_OnProgressChange;
            fileLeakerCore.OnSecretFound += FileLeakerCore_OnSecretFound;
            fileLeakerCore.OnStarted += FileLeakerCore_OnStarted;
            fileLeakerCore.OnTotalFilesCount += FileLeakerCore_OnTotalFilesCount;

            

            await AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),    // Task description
                    new ProgressBarColumn(),        // Progress bar
                    new PercentageColumn(),         // Percentage
                    new ElapsedTimeColumn(),      // Remaining time
                    new SpinnerColumn(),            // Spinner
                })
                .StartAsync(async ctx =>
                {
                    CurrentContext = ctx;

                    CurrentProgressTask = ctx.AddTask("[green bold]Searching Files, please wait...[/]");
                    CurrentProgressTask.MaxValue = 100;

                    CurrentFileTask = ctx.AddTask("[blue bold]Searching Files...[/]");
                    CurrentFileTask.IsIndeterminate = true;

                    CurrentSecretsTask = ctx.AddTask("[yellow bold]Secrets found: 0[/]");
                    CurrentSecretsTask.IsIndeterminate = true;


                    fileLeakerCore.DoSearch(settings.Path);

                    while (!ctx.IsFinished)
                    {
                        await Task.Delay(250);
                    }


                });

            ProcessDataAndFinishProcess();
        }

        private void ProcessDataAndFinishProcess()
        {
            StringBuilder contentOutput = new StringBuilder();
            try
            {
                foreach (var secretResult in _SecretResultList)
                {
                    _console.MarkupLine($"[bold yellow][[+]] [/][bold yellow]File: {secretResult.FilePath.NormalizeString()}[/]");
                    contentOutput.AppendLine($"[+] File: {secretResult.FilePath}");
                    foreach (var matchResult in secretResult.MatchResultList)
                    {
                        _console.MarkupLine($"[bold yellow] | -[/][bold] Key: {matchResult.Name.NormalizeString()} | Secret: {matchResult.Result.NormalizeString()}[/]");
                        //_console.MarkupLine($"[bold yellow] | -[/][bold] [/]");
                        _console.MarkupLine($"[bold yellow] | -[/][bold] Content: {matchResult.Content.NormalizeString()}[/]");
                        _console.MarkupLine($"[bold yellow] | [/]");
                        contentOutput.AppendLine($" | - Key: {matchResult.Name} | Secret: {matchResult.Result}");
                        contentOutput.AppendLine($" | - Content: {matchResult.Content.TrimStart().TrimEnd()}");
                        contentOutput.AppendLine($" | ");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on console write: " + ex.ToString());
            }
            
            File.WriteAllText($"{Environment.CurrentDirectory}/fileleaks_output.txt", contentOutput.ToString());
            _console.MarkupLine($"[bold] Report saved in {Environment.CurrentDirectory}/fileleaks_output.txt[/]");
        }

        private static IEnumerable<Core.Models.SecretResult> _SecretResultList;
        private void FileLeakerCore_OnFinished(object sender, IEnumerable<Core.Models.SecretResult> secretResultList)
        {
            CurrentFileTask.Value = 100;
            CurrentSecretsTask.Value = 100;
            _SecretResultList = secretResultList;
        }

        private static void FileLeakerCore_OnStarted(object sender, EventArgs e)
        {
            CurrentFileTask.Value = 0;
            CurrentSecretsTask.Value = 0;
            TotalSecretsFound = 0;
        }



        private static void FileLeakerCore_OnTotalFilesCount(object sender, int e)
        {
            CurrentProgressTask.Description = $"[green bold]Lookin for secrets in {e} file(s), please wait...[/]";
        }


        private static int TotalSecretsFound = 0;
        private static void FileLeakerCore_OnSecretFound(object sender, Core.Models.MatchResult e)
        {
            TotalSecretsFound++;
            CurrentSecretsTask.Description = $"[yellow bold]Secrets found: {TotalSecretsFound}[/]";

            StringBuilder contentOutput = new StringBuilder();

            contentOutput.AppendLine($" | - Key: {e.Name} | Secret: {e.Result}");
            contentOutput.AppendLine($" | - Content: {e.Content.TrimStart().TrimEnd()}");
            contentOutput.AppendLine($" | ");

            //Console.WriteLine("Secret found: " + e.FilePath);
        }

        private static void FileLeakerCore_OnProgressChange(object sender, Core.Models.Events.FileScanProgress e)
        {
            CurrentFileTask.Description = $"[blue bold]File {Path.GetFileName(e.Filename)}[/]";
            CurrentFileTask.Value = e.ProgressPercentage;
            CurrentSecretsTask.Value = e.ProgressPercentage;
            CurrentProgressTask.Value = e.ProgressPercentage;
        }
    }
}
