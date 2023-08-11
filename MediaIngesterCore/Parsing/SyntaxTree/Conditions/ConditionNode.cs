namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public abstract class ConditionNode : SyntaxNode
{
    public ConditionNode(ExpressionNode? l, ExpressionNode? r)
    {
        this.L = l;
        this.R = r;
    }

    public ExpressionNode? L { get; }
    public ExpressionNode? R { get; }
}