namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public class ContainsNode : ConditionNode
{
    public ContainsNode(ExpressionNode l, ExpressionNode r) : base(l, r)
    {
    }
}