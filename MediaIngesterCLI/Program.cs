using MediaIngesterCore.Parsing.SyntaxTree;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Ingesting;
using System.CommandLine;
using System.ComponentModel;

namespace MediaIngesterCLI
{
    internal static class Program
    {
        private static int Main(string[] args)
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
                
            RootCommand rootCommand = new("A simple command line ingest tool");

            Command ingestCommand = new("ingest", "Ingest a folder")
            {
                sourcePath,
                destinationPath,
                rulesPath,
            };
            rootCommand.AddCommand(ingestCommand);
            
            ingestCommand.SetHandler(async (source, destination, rules) =>
            {
                await Ingest(source, destination, rules);
            }, sourcePath, sourcePath, rulesPath);
            return rootCommand.InvokeAsync(args).Result;
        }

        private static async Task Ingest(DirectoryInfo sourcePath, DirectoryInfo destinationPath, FileInfo rulesPath)
        {
            
            Parser parser = new Parser();
            SyntaxNode rules;
            try
            {
                rules = parser.Parse(await File.ReadAllTextAsync(rulesPath.FullName));
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Error parsing rule file \"{rulesPath.FullName}\": {e.Message}");
                return;
            }
            IngestJob job = new IngestJob(sourcePath.FullName, destinationPath.FullName,rules);
            if (!job.ScanDirectory())
            {
                Console.WriteLine($"No files found in source directory \"{sourcePath}\"");
                return;
            }

            Ingester ingester = new Ingester(job);
            ingester.FileIngestCompleted += OnFileIngestCompleted;
            
            Console.WriteLine($"Ingesting from {sourcePath} to {destinationPath}");
            await ingester.Ingest();
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
}