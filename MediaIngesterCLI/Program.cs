using System.CommandLine;
using MediaIngesterCLI.Commands;

namespace MediaIngesterCLI;

public static class Program
{
    public static int Main(string[] args)
    {
        RootCommand rootCommand = new RootCommand("A simple command line ingest tool");

        rootCommand.AddCommand(new IngestCommand());
        rootCommand.AddCommand(new PreviewCommand());
        rootCommand.AddCommand(new DebugCommand());
        return rootCommand.InvokeAsync(args).Result;
    }
}