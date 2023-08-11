using System.CommandLine;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Parsing.SyntaxTree;

namespace MediaIngesterCLI.Commands;

public class PreviewFileCommand : Command
{
    public PreviewFileCommand() : base("file", "Prints the ingest location of a file without ingesting it")
    {
        Argument<FileInfo> previewFilePath = new(
            "file",
            "The file to preview");

        Argument<FileInfo> rulesPath = new(
            "rules",
            "The rules file to use while ingesting");

        this.AddArgument(previewFilePath);
        this.AddArgument(rulesPath);
        this.SetHandler(context =>
        {
            FileInfo file = context.ParseResult.GetValueForArgument(previewFilePath);
            FileInfo rules = context.ParseResult.GetValueForArgument(rulesPath);
            int exitCode = PreviewFile(file, rules);
            context.ExitCode = exitCode;
        });
    }

    private static int PreviewFile(FileInfo filePath, FileInfo rulesPath)
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

        Evaluator evaluator = new(filePath.FullName);
        Console.WriteLine((string)evaluator.Evaluate(rules));
        return 0;
    }
}