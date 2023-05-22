using MediaIngesterCore.Parsing.SyntaxTree;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Ingesting;

namespace MediaIngesterCLI
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string sourcePath = args[0];
            string destinationPath = args[1];
            string rulePath = args[2];
            
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"Source directory \"{sourcePath}\" does not exist");
                return;
            }
            if (!Directory.Exists(destinationPath))
            {
                Console.WriteLine($"Destination directory \"{destinationPath}\" does not exist");
                return;
            }
            if (!File.Exists(rulePath))
            {
                Console.WriteLine($"Rule file \"{rulePath}\" does not exist");
                return;
            }
            Parser parser = new Parser();
            SyntaxNode rules;
            try
            {
                rules = parser.Parse(File.ReadAllText(rulePath));
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Error parsing rule file \"{rulePath}\": {e.Message}");
                return;
            }
            IngestJob job = new IngestJob(sourcePath, destinationPath, rules);
            if (!job.ScanDirectory())
            {
                Console.WriteLine($"No files found in source directory \"{sourcePath}\"");
                return;
            }

            Ingester ingester = new Ingester(job);
            Task task = ingester.Ingest();
            Console.WriteLine($"Ingesting from {sourcePath} to {destinationPath}");
            task.Wait();
            
        }
    }
}