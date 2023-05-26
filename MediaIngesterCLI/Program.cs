using System.CommandLine;
using static MediaIngesterCLI.CommandHandlers;

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
            
            Argument<FileInfo> previewFilePath = new(
                name: "file",
                description: "The file to preview");

            RootCommand rootCommand = new("A simple command line ingest tool");

            Command ingestCommand = new("ingest", "Ingest a folder")
            {
                sourcePath,
                destinationPath,
                rulesPath
            };
            rootCommand.AddCommand(ingestCommand);
            
            CancellationTokenSource tokenSource = new();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Cancelling ingest...");
                tokenSource.Cancel();
                eventArgs.Cancel = true;
            };
            CancellationToken cToken = tokenSource.Token;
            ingestCommand.SetHandler(async (source, destination,rules) =>
            { 
                await Ingest(source, destination, rules,cToken);
            }, sourcePath, sourcePath, rulesPath);
            
            Command previewCommand = new("preview", "Preview the results of an ingest operation without executing it");
            Command previewFileCommand = new("file", "Prints the ingest location of a file without ingesting it")
            {
                previewFilePath,
                rulesPath
            };
            previewFileCommand.SetHandler((file,rules) =>
            {
                Console.WriteLine(PreviewFile(file, rules));
            }, previewFilePath, rulesPath);
            previewCommand.AddCommand(previewFileCommand);
            rootCommand.AddCommand(previewCommand);
            Command previewDirectoryCommand = new("directory", "Prints the file tree that would be created by " +
                                                               "the rules file if all rules matched at least once")
            {
                rulesPath
            };
            previewDirectoryCommand.SetHandler((rules) =>
            {
                Console.WriteLine(PreviewDirectory(rules));
            }, rulesPath);
            previewCommand.AddCommand(previewDirectoryCommand);

            return rootCommand.InvokeAsync(args).Result;
        }
        
        
        
        
    }
}