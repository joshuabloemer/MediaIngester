using System.Text;
using MediaIngesterCore.Ingesting;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Parsing.SyntaxTree;

namespace MediaIngesterCLI;

internal class CommandHandlers
{
        internal static string PreviewDirectory(FileInfo rulesPath)
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
        internal static string PreviewFile(FileInfo filePath, FileInfo rulesPath)
        {
            Parser parser = new Parser();
            SyntaxNode rules;
            try
            {
                rules = parser.Parse( File.ReadAllText(rulesPath.FullName));
            }
            catch (FormatException e)
            {
                Console .WriteLine($"Error parsing rule file \"{rulesPath.FullName}\": {e.Message}");
                return null;
            }

            Evaluator evaluator = new Evaluator(filePath.FullName);
            return (string)evaluator.Evaluate(rules);
        }
        internal static async Task Ingest(DirectoryInfo sourcePath, DirectoryInfo destinationPath, FileInfo rulesPath, CancellationToken token)
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
            await ingester.Ingest(token, new ManualResetEvent(true));
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