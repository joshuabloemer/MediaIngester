namespace MediaIngesterCore.Parsing.SyntaxTree;

public class ValueNode : ExpressionNode
{
    public ValueNode(List<SyntaxNode> value)
    {
        this.Value = value;
    }

    public List<SyntaxNode> Value { get; }
}