namespace MediaIngesterCore.Parsing.SyntaxTree;

public class AssignNode : SyntaxNode
{
    public AssignNode(string name, ExpressionNode value)
    {
        this.Name = name;
        this.Value = value;
    }

    public string Name { get; }
    public ExpressionNode Value { get; }
}