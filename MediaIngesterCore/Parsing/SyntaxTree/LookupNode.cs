namespace MediaIngesterCore.Parsing.SyntaxTree;

public class LookupNode : ExpressionNode
{
    public LookupNode(string name)
    {
        this.Name = name;
    }

    public string Name { get; }

    public override string ToString()
    {
        return base.ToString() + " " + this.Name;
    }
}