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
            Console.WriteLine($"Error parsing rule file \"{rulesPath.FullName}\": {e.Message}");
            return 1;
        }

        List<string> paths = (List<string>)FileTreeEvaluator.Evaluate(rules.Block);
        paths = paths.Distinct().ToList();
        paths.Sort();
        // TreeBuilder builder = new("destination", null);
        // foreach (string path in paths)
        // {
        //     List<string> items = path.Split("/").ToList();
        //     items.RemoveAt(0);
        //     TreeBuilder.TreeStruct(builder, items);
        // }

        // StringBuilder output = new();
        // builder.PrintTree(output, true);
        // Console.WriteLine(output.ToString());
        Tree? root = new("Destination");
        List<List<string>> splitPaths = new();
        foreach (string path in paths)
        {
            List<string> splitPath = path.Split("/").ToList();
            splitPath.RemoveAt(0);
            splitPaths.Add(splitPath);
        }

        Utils.CreateTreeRecursive(splitPaths, root);

        // // Add some nodes
        // TreeNode? foo = root.AddNode("[yellow]Foo[/]");
        // TreeNode? table = foo.AddNode(new Table()
        //     .RoundedBorder()
        //     .AddColumn("First")
        //     .AddColumn("Second")
        //     .AddRow("1", "2")
        //     .AddRow("3", "4")
        //     .AddRow("5", "6"));
        //
        // table.AddNode("[blue]Baz[/]");
        // foo.AddNode("Qux");
        //
        // TreeNode? bar = root.AddNode("[yellow]Bar[/]");
        // bar.AddNode(new Calendar(2020, 12)
        //     .AddCalendarEvent(2020, 12, 12)
        //     .HideHeader());

        // Render the tree
        AnsiConsole.Write(root);
        return 0;
    }
}