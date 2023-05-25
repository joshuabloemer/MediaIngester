using MediaIngesterCore.Parsing.SyntaxTree;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Ingesting;
using System.CommandLine;
using System.ComponentModel;
using System.Text;

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
            
            // Argument<DirectoryInfo> previewDirectoryPath = new(
            //     name: "directory",
            //     description: "The directory to preview");
                
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

            Command previewCommand = new("preview", "Preview a folder");
            Command previewFileCommand = new("file", "Preview a file")
            {
                previewFilePath,
                rulesPath
            };
            previewFileCommand.SetHandler((file,rules) =>
            {
                Console.WriteLine(previewFile(file, rules));
            }, previewFilePath, rulesPath);
            previewCommand.AddCommand(previewFileCommand);
            rootCommand.AddCommand(previewCommand);
            
            Command previewDirectoryCommand = new("directory", "Preview a directory")
            {
                rulesPath
            };
            previewDirectoryCommand.SetHandler((rules) =>
            {
                Console.WriteLine(previewDirectory(rules));
            }, rulesPath);
            previewCommand.AddCommand(previewDirectoryCommand);



            return rootCommand.InvokeAsync(args).Result;
        }
        private static string previewDirectory(FileInfo rulesPath)
        {
            Parser parser = new Parser();
            ProgramNode rules;
            try
            {
                rules = parser.Parse( File.ReadAllText(rulesPath.FullName));
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Error parsing rule file \"{rulesPath.FullName}\": {e.Message}");
                return null;
            }

            List<String> paths =(List<String>)FileTreeEvaluator.Evaluate(rules.Block);
            paths = paths.Distinct().ToList();
            paths.Sort();
            TreeBuilder builder = new TreeBuilder("destination",null);
            foreach (var path in paths)
            {
                List<String> items = path.Split("/").ToList();
                items.RemoveAt(0);
                TreeBuilder.TreeStruct(builder, items);
            }
            
            StringBuilder output = new StringBuilder();
            builder.PrintTree(output, true);
            return output.ToString();
        }
        private static string previewFile(FileInfo filePath, FileInfo rulesPath)
        {
            Parser parser = new Parser();
            SyntaxNode rules;
            try
            {
                rules = parser.Parse( File.ReadAllText(rulesPath.FullName));
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Error parsing rule file \"{rulesPath.FullName}\": {e.Message}");
                return null;
            }

            Evaluator evaluator = new Evaluator(filePath.FullName);
            return (string)evaluator.Evaluate(rules);
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