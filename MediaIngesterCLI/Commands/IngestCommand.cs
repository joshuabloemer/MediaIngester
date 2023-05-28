using System.CommandLine;
using MediaIngesterCore.Ingesting;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Parsing.SyntaxTree;

namespace MediaIngesterCLI.Commands;

internal class IngestCommand : Command
{
    public IngestCommand() : base("ingest", "Ingest a folder")
    {
        Argument<DirectoryInfo> sourcePath = new(
            name: "source",
            description: "The source directory to ingest from");

        Argument<DirectoryInfo> destinationPath = new(
            name: "destination",
            description: "The destination directory to ingest to");

        Argument<FileInfo> rulesPath = new(
            name: "rules",
            description: "The rules file to use while ingesting");

        this.AddArgument(sourcePath);
        this.AddArgument(destinationPath);
        this.AddArgument(rulesPath);
        
        CancellationTokenSource tokenSource = new();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Cancelling ingest...");
            tokenSource.Cancel();
            eventArgs.Cancel = true;
        };
        CancellationToken cToken = tokenSource.Token;
        
        this.SetHandler(async (context) =>
        {
            DirectoryInfo source = context.ParseResult.GetValueForArgument(sourcePath);
            DirectoryInfo destination = context.ParseResult.GetValueForArgument(destinationPath);
            FileInfo rules = context.ParseResult.GetValueForArgument(rulesPath);
            int exitCode = await Ingest(source, destination, rules,cToken);
            context.ExitCode = exitCode;
        });
    }
    
    private static async Task<int> Ingest(DirectoryInfo sourcePath, DirectoryInfo destinationPath, FileInfo rulesPath, CancellationToken token)
    {
            
        Parser parser = new Parser();
        SyntaxNode rules;
        try
        {
            rules = parser.Parse(await File.ReadAllTextAsync(rulesPath.FullName, token));
        }
        catch (FormatException e)
        {
            Console.WriteLine($"Error parsing rule file \"{rulesPath.FullName}\": {e.Message}");
            return 1;
        }
        IngestJob job = new IngestJob(sourcePath.FullName, destinationPath.FullName,rules);
        if (!job.ScanDirectory())
        {
            Console.WriteLine($"No files found in source directory \"{sourcePath}\"");
            return 1;
        }

        Ingester ingester = new Ingester(job);
        ingester.FileIngestCompleted += OnFileIngestCompleted;
            
        Console.WriteLine($"Ingesting from {sourcePath} to {destinationPath}");
        await ingester.Ingest(token, new ManualResetEvent(true));
        return 0;
    }
    private static void OnFileIngestCompleted(object? sender, FileIngestCompletedEventArgs e)
    {
        string message = $"File {e.FileNumber} ({e.FilePath}) ";
        if (e.Skipped)
        {
            message += "was skipped";
        }
        else if (e.Renamed)
        {
            message += $"was copied and renamed to {e.NewPath}";
        }
        else
        {
            message += $"was copied to {e.NewPath}";
        }
        Console.WriteLine(message);
    }
}
