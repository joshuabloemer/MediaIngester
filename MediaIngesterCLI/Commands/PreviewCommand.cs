using System.CommandLine;

namespace MediaIngesterCLI.Commands;

public class PreviewCommand : Command
{
    public PreviewCommand() : base("preview", "Preview the results of an ingest operation without executing it")
    {
        this.AddCommand(new PreviewDirectoryCommand());
        this.AddCommand(new PreviewFileCommand());
    }
}