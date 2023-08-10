namespace MediaIngesterCore.Parsing.SyntaxTree;

public class AssignNode : SyntaxNode
{
    public AssignNode(string name, SyntaxNode value)
    {
        this.Value = value;
    }

    public SyntaxNode Value { get; }
}