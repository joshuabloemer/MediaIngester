using System.Text.RegularExpressions;
using MediaIngesterCore.Parsing.Exceptions;
using MediaIngesterCore.Parsing.SyntaxTree;
using MediaIngesterCore.Parsing.SyntaxTree.Conditions;
using MetadataExtractor;
using static MediaIngesterCore.Utils;
using Directory = MetadataExtractor.Directory;

namespace MediaIngesterCore.Parsing;

public class Evaluator
{
    private readonly Dictionary<string, Dictionary<string, string>> metadata = new();

    private readonly Dictionary<string, string> variables = new();

    public Evaluator(string filePath)
    {
        DateTime dateTaken = GetDateTaken(filePath);

        this.variables["year"] = dateTaken.Year.ToString();
        this.variables["month"] = dateTaken.Month.ToString();
        this.variables["day"] = dateTaken.Day.ToString();
        this.variables["hour"] = dateTaken.Hour.ToString();
        this.variables["minute"] = dateTaken.Minute.ToString();
        this.variables["second"] = dateTaken.Second.ToString();
        this.variables["file_name"] = Path.GetFileNameWithoutExtension(filePath);
        this.variables["extension"] = Path.GetExtension(filePath)[1..];
        this.variables["path"] = filePath;
        try
        {
            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
            foreach (Directory directory in directories)
            {
                this.metadata[directory.Name] = new Dictionary<string, string>();
                foreach (Tag tag in directory.Tags) this.metadata[directory.Name][tag.Name] = tag.Description!;
            }
        }
        catch (ImageProcessingException)
        {
        }
    }

    public bool Ignore { get; private set; }

    public bool RuleMatched { get; private set; }

    public string Evaluate(ProgramNode program)
    {
        if (program.VarBlock is not null)
            this.Evaluate(program.VarBlock);

        return Regex.Replace(this.Evaluate(program.Block) ?? string.Empty, @"[\\/]",
            Path.DirectorySeparatorChar.ToString());
    }

    private void Evaluate(VarBlockNode varBlock)
    {
        foreach (AssignNode assign in varBlock.Statements) this.Evaluate(assign);
    }

    private void Evaluate(AssignNode assign)
    {
        this.variables[assign.Name] = this.Evaluate(assign.Value);
    }

    private string Evaluate(ExpressionNode expression)
    {
        return expression switch
        {
            PathPartNode p => this.Evaluate(p),
            MetadataNode m => this.Evaluate(m),
            LookupNode l => this.Evaluate(l),
            ValueNode v => this.Evaluate(v),
            _ => throw new NotImplementedException(expression.ToString())
        };
    }

    private string Evaluate(PathPartNode pathPart)
    {
        string[] pathParts = this.variables["path"].Split("\\");
        if (pathPart.Part >= 0)
            return pathParts[pathPart.Part];
        return pathParts[^Math.Abs(pathPart.Part)];
    }

    private string Evaluate(MetadataNode metadataNode)
    {
        if (!this.metadata.TryGetValue(metadataNode.Directory, out Dictionary<string, string>? directory))
            throw new MetadataNotFoundException("Metadata directory not found: " + metadataNode.Directory);
        if (!directory.TryGetValue(metadataNode.Tag, out string? value))
            throw new MetadataNotFoundException("Metadata tag not found: " + metadataNode.Tag);
        return value;
    }

    private string Evaluate(LookupNode lookup)
    {
        if (this.variables.TryGetValue(lookup.Name, out string? value)) return value;
        throw new VariableNotDefinedException("Variable not defined: " + lookup.Name);
    }

    private string Evaluate(ValueNode value)
    {
        return value.Value.Aggregate("", (current, v) => current + v switch
        {
            LookupNode l => this.Evaluate(l),
            LiteralNode l => this.Evaluate(l),
            _ => throw new NotImplementedException(v.ToString())
        });
    }

    private string Evaluate(LiteralNode literal)
    {
        return literal.Value;
    }

    public string? Evaluate(BlockNode block)
    {
        foreach (RuleNode statement in block.Statements)
        {
            string? result = this.Evaluate(statement);
            if (result is not null) return result;
        }

        return null;
    }

    private string? Evaluate(RuleNode rule)
    {
        if (this.Ignore)
            return null;
        string? result = null;
        if (this.Evaluate(rule.Condition))
        {
            switch (rule.Path)
            {
                case ExpressionNode e:
                    result = this.Evaluate(e);
                    break;
                case IgnoreNode:
                    this.Ignore = true;
                    return null;
                default:
                    throw new NotImplementedException(rule.Path.ToString());
            }

            this.RuleMatched = true;
            BlockNode? indent = rule.GetIndent();
            if (indent is not null) result = Path.Join(result, this.Evaluate(indent));
        }
        else if (rule.Under is not null)
        {
            result = this.Evaluate(rule.Under);
        }

        return result;
    }

    private bool Evaluate(ConditionNode condition)
    {
        return condition switch
        {
            NotNode n => !this.Evaluate(n.Condition),
            EqualsNode e => this.Evaluate(e),
            NotEqualsNode n => this.Evaluate(n),
            ContainsNode c => this.Evaluate(c),
            MatchesNode m => this.Evaluate(m),
            AnyNode => true,
            _ => throw new NotImplementedException(condition.ToString())
        };
    }

    private bool Evaluate(EqualsNode equals)
    {
        return this.Evaluate(equals.L!) == this.Evaluate(equals.R!);
    }

    private bool Evaluate(NotEqualsNode notEquals)
    {
        return this.Evaluate(notEquals.L!) != this.Evaluate(notEquals.R!);
    }

    private bool Evaluate(ContainsNode contains)
    {
        return this.Evaluate(contains.L!).Contains(this.Evaluate(contains.R!));
    }

    private bool Evaluate(MatchesNode matches)
    {
        return Regex.IsMatch(this.Evaluate(matches.L!), this.Evaluate(matches.R!));
    }
}