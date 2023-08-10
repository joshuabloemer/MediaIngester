using MediaIngesterCore.Parsing.SyntaxTree;
using MediaIngesterCore.Parsing.SyntaxTree.Conditions;
using MetadataExtractor;
using static MediaIngesterCore.Utils;
using Directory = MetadataExtractor.Directory;

namespace MediaIngesterCore.Parsing;

public class Evaluator
{
    public Evaluator(string filePath)
    {
        this.FilePath = filePath;
        this.DateTaken = GetDateTaken(filePath);

        try
        {
            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
            foreach (Directory directory in directories)
            {
                this.Metadata[directory.Name] = new Dictionary<string, string>();
                foreach (Tag tag in directory.Tags) this.Metadata[directory.Name][tag.Name] = tag.Description!;
            }
        }
        catch (ImageProcessingException)
        {
        }
    }

    public string FilePath { get; }

    public Dictionary<string, Dictionary<string, string>> Metadata { get; } = new();

    public DateTime DateTaken { get; }

    public bool RuleMatched { get; private set; }

    public object Evaluate(SyntaxNode node)
    {
        switch (node)
        {
            case ProgramNode p: return this.Evaluate(p.Block);
            case StringNode n: return n.Value;
            case BlockNode b: return this.Block(b);
            case RuleNode r: return this.Rule(r);
            case AnyNode: return true;
            case EqualsNode e: return this.Equals(e);
            case NotEqualsNode n: return this.Not(n);
            case ContainsNode c: return this.ContainsNode(c);
            case MetadataNode m: return this.MetadataNode(m);
            case PathPartNode p: return this.PathPartNode(p);
            case EmptyNode: return null;
        }

        throw new Exception($"Unknown node type {node.GetType()}");
    }

    private object ContainsNode(ContainsNode c)
    {
        string lhs = (string)this.Evaluate(c.L);
        string rhs = (string)this.Evaluate(c.R);
        return lhs.Contains(rhs);
    }

    private object PathPartNode(PathPartNode p)
    {
        string[] pathParts = this.FilePath.Split("\\");
        if (p.Part >= 0)
            return pathParts[p.Part];
        return pathParts[^Math.Abs(p.Part)];
    }

    private object MetadataNode(MetadataNode m)
    {
        Dictionary<string, string> directory;
        string tag;
        if (this.Metadata.TryGetValue(m.Directory, out directory))
            if (directory.TryGetValue(m.Tag, out tag))
                return tag;
        return "null";
    }


    private object Not(NotEqualsNode n)
    {
        return Convert.ToString(this.Evaluate(n.L)) != Convert.ToString(this.Evaluate(n.R));
    }

    private object Equals(EqualsNode e)
    {
        return Convert.ToString(this.Evaluate(e.L)) == Convert.ToString(this.Evaluate(e.R));
    }

    private string? Rule(RuleNode r)
    {
        string? result = null;
        if ((bool)this.Evaluate(r.Condition))
        {
            result = (string)this.Evaluate(r.Path);
            this.RuleMatched = true;
            SyntaxNode indent = r.GetIndent();
            if (indent is not null) result = Path.Join(result, Convert.ToString(this.Evaluate(indent)));
        }
        else if (r.Under is not EmptyNode)
        {
            result = (string)this.Evaluate(r.Under);
        }

        return result;
    }

    private string? Block(BlockNode b)
    {
        string? result = null;
        foreach (SyntaxNode statement in b.Statements)
        {
            result = (string)this.Evaluate(statement); // use cast instead of Convert.ToString to allow for null return
            if (result is not null) return result;
        }

        return null;
    }
}