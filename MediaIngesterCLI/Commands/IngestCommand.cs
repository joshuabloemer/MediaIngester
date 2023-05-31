using System.CommandLine;
using MediaIngesterCore.Ingesting;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Parsing.SyntaxTree;
using Spectre.Console;

namespace MediaIngesterCLI.Commands;

internal class IngestCommand : Command
{
    private IngestJob? job;
    private ProgressContext? progressContext;
    private ProgressTask? progressTask;

    public IngestCommand() : base("ingest", "Ingest a folder")
    {
        Argument<DirectoryInfo> sourcePath = new(
            "source",
            "The source directory to ingest from");

        Argument<DirectoryInfo> destinationPath = new(
            "destination",
            "The destination directory to ingest to");

        Argument<FileInfo> rulesPath = new(
            "rules",
            "The rules file to use while ingesting");

        AddArgument(sourcePath);
        AddArgument(destinationPath);
        AddArgument(rulesPath);

        CancellationTokenSource tokenSource = new();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Cancelling ingest...");
            tokenSource.Cancel();
            eventArgs.Cancel = true;
        };
        CancellationToken cToken = tokenSource.Token;

        this.SetHandler(async context =>
        {
            DirectoryInfo source = context.ParseResult.GetValueForArgument(sourcePath);
            DirectoryInfo destination = context.ParseResult.GetValueForArgument(destinationPath);
            FileInfo rules = context.ParseResult.GetValueForArgument(rulesPath);
            int exitCode = await Ingest(source, destination, rules, cToken);
            context.ExitCode = exitCode;
        });
    }

    private async Task<int> Ingest(DirectoryInfo sourcePath, DirectoryInfo destinationPath, FileInfo rulesPath,
        CancellationToken token)
    {
        Parser parser = new();
        SyntaxNode rules;
        try
        {
            rules = parser.Parse(await File.ReadAllTextAsync(rulesPath.FullName, token));
        }
        catch (FormatException e)
        {
            await Console.Error.WriteLineAsync($"Error parsing rule file \"{rulesPath.FullName}\": {e.Message}");
            return 1;
        }

        job = new IngestJob(sourcePath.FullName, destinationPath.FullName, rules);
        if (!job.ScanDirectory())
        {
            await Console.Error.WriteLineAsync($"No files found in source directory \"{sourcePath}\"");
            return 1;
        }

        Ingester ingester = new(job);
        ingester.FileIngestCompleted += OnFileIngestCompleted;
        ingester.FileIngestStarted += OnFileIngestStarted;

        AnsiConsole.WriteLine($"Ingesting from {sourcePath} to {destinationPath}");
        await AnsiConsole.Progress()
            .AutoRefresh(false)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(),
                new RemainingTimeColumn(), new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                progressContext = ctx;
                string message =
                    $"{"0".PadLeft((int)Math.Floor(Math.Log10(job.TotalFiles) + 1), '0')}/{job.TotalFiles}";
                ProgressTask task = ctx.AddTask($"[green]Ingesting file {message}[/]");
                progressTask = task;
                IProgress<double> progress = new Progress<double>(ReportProgress);
                await ingester.Ingest(token, new ManualResetEvent(true), progress);
            });
        return 0;
    }

    private void ReportProgress(double progress)
    {
        progressTask?.Value(progress * 100);
        // this.task.Description = $"[green]Ingesting file {progress*100:0.00}%[/]";
        // task.Description = $"[green]Ingesting file {progress*100:0.00}%[/]";
    }

    private void OnFileIngestStarted(object? sender, FileIngestStartedEventArgs e)
    {
        string message =
            $"{(e.FileNumber + 1).ToString().PadLeft((int)Math.Floor(Math.Log10(job.TotalFiles) + 1), '0')}/{job.TotalFiles}";
        progressTask!.Description = $"[green]Ingesting file {message}[/]";
    }

    private void OnFileIngestCompleted(object? sender, FileIngestCompletedEventArgs e)
    {
        string message = $"File {e.FileNumber + 1} ({e.FilePath}) ";
        if (e.Skipped)
            message += "was skipped";
        else if (e.Renamed)
            message += $"was copied and renamed to {e.NewPath}";
        else
            message += $"was copied to {e.NewPath}";
        AnsiConsole.WriteLine(message);
        progressContext?.Refresh();
    }
}