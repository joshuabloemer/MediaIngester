namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public class EqualsNode : ConditionNode
{
    public EqualsNode(ExpressionNode l, ExpressionNode r) : base(l, r)
    {
    }
}