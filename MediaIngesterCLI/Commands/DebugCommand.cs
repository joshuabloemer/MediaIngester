using System.CommandLine;
using MediaIngesterCore.Parsing;
using MediaIngesterCore.Parsing.SyntaxTree;

namespace MediaIngesterCLI.Commands;

internal class DebugCommand : Command
{
    public DebugCommand() : base("debug", "prints the syntax tree of the provided rules file")
    {
        Argument<FileInfo> rulesPath = new Argument<FileInfo>("rules", "The rules file to debug");

        this.AddArgument(rulesPath);

        this.SetHandler(context =>
        {
            FileInfo ruleFile = context.ParseResult.GetValueForArgument(rulesPath);
            Parser parser = new Parser();
            
            try
            {
                SyntaxNode rules = parser.Parse(File.ReadAllText(ruleFile.FullName));
                Console.WriteLine(rules.PrettyPrint());
            }
            catch (FormatException e)
            {
                Console.Error.WriteLine($"Error parsing rule file \"{ruleFile.FullName}\": {e.Message}");
            }
        });
    }
}