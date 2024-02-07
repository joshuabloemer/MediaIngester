namespace MediaIngesterCore.Parsing.SyntaxTree;

public class PathPartNode : ExpressionNode
{
    public PathPartNode(int part)
    {
        this.Part = part;
    }

    public int Part { get; }
}