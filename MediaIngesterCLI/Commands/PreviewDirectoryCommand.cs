using System.CommandLine;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Parsing.SyntaxTree;
using Spectre.Console;

namespace MediaIngesterCLI.Commands;

public class PreviewDirectoryCommand : Command
{
    public PreviewDirectoryCommand() : base("directory",
        "Prints the ingest location of a directory without ingesting it")
    {
        Argument<FileInfo> rulesPath = new(
            "rules",
            "The rules file to use while ingesting");

        AddArgument(rulesPath);
        this.SetHandler(context =>
        {
            FileInfo rules = context.ParseResult.GetValueForArgument(rulesPath);
            int exitCode = PreviewDirectory(rules);
            context.ExitCode = exitCode;
        });
    }

    private static int PreviewDirectory(FileInfo rulesPath)
    {
        Parser parser = new();
        ProgramNode rules;
        try
        {
            rules = parser.Parse(File.ReadAllText(rulesPath.FullName));
        }
        catch (FormatException e)
        {
            Console.Error.WriteLine($"Error parsing rule file \"{rulesPath.FullName}\": {e.Message}");
            return 1;
        }

        List<string> paths = (List<string>)FileTreeEvaluator.Evaluate(rules.Block);

        paths.Sort();
        Tree? root = new("Destination");
        List<List<string>> splitPaths = new();
        foreach (string path in paths)
        {
            List<string> splitPath = path.Split("/").ToList();
            splitPath.RemoveAt(0);
            splitPaths.Add(splitPath);
        }

        Utils.CreateTreeRecursive(splitPaths, root);
        AnsiConsole.Write(root);
        return 0;
    }
}