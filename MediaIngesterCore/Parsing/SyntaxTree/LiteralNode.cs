namespace MediaIngesterCore.Parsing.SyntaxTree;

public class LiteralNode : SyntaxNode
{
    public LiteralNode(string value)
    {
        this.Value = value;
    }

    public string Value { get; }

    public override string ToString()
    {
        return "literal: " + this.Value;
    }
}