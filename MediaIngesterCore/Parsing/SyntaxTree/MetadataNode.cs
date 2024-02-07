namespace MediaIngesterCore.Parsing.SyntaxTree;

public class MetadataNode : ExpressionNode
{
    public MetadataNode(string directory, string tag)
    {
        this.Directory = directory;
        this.Tag = tag;
    }

    public string Directory { get; }

    public string Tag { get; }

    public override string ToString()
    {
        return base.ToString() + " " + this.Directory + " - " + this.Tag;
    }
}