namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public class NotEqualsNode : ConditionNode
{
    public NotEqualsNode(ExpressionNode l, ExpressionNode r) : base(l, r)
    {
    }
}