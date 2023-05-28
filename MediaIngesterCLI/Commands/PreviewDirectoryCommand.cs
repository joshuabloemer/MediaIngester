using System.CommandLine;
using System.Text;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Parsing.SyntaxTree;

namespace MediaIngesterCLI.Commands;

public class PreviewDirectoryCommand : Command
{
    public PreviewDirectoryCommand() : base("directory", "Prints the ingest location of a directory without ingesting it")
    {
        Argument<FileInfo> rulesPath = new(
            name: "rules",
            description: "The rules file to use while ingesting");
        
        this.AddArgument(rulesPath);
        this.SetHandler((rules) =>
        {
            Console.WriteLine(PreviewDirectory(rules));
        },  rulesPath);
    }

    private static string PreviewDirectory(FileInfo rulesPath)
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
}