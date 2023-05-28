using System.CommandLine;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Parsing.SyntaxTree;

namespace MediaIngesterCLI.Commands;

public class PreviewFileCommand : Command
{
    public PreviewFileCommand() : base("file", "Prints the ingest location of a file without ingesting it")
    {
        Argument<FileInfo> previewFilePath = new(
            name: "file",
            description: "The file to preview");
        
        Argument<FileInfo> rulesPath = new(
            name: "rules",
            description: "The rules file to use while ingesting");
        this.AddArgument(previewFilePath);
        this.AddArgument(rulesPath);
        this.SetHandler((file,rules) =>
        {
            Console.WriteLine(PreviewFile(file, rules));
        }, previewFilePath, rulesPath);
    }

    private static string PreviewFile(FileInfo filePath, FileInfo rulesPath)
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
}