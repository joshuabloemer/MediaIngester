using System.CommandLine;
using MediaIngesterCLI.Commands;


namespace MediaIngesterCLI
{
    
    internal static class Program
    {
        private static int Main(string[] args)
        {

            RootCommand rootCommand = new("A simple command line ingest tool");

            rootCommand.AddCommand(new IngestCommand());
            rootCommand.AddCommand(new PreviewCommand());
            return rootCommand.InvokeAsync(args).Result;
        }
    }
}