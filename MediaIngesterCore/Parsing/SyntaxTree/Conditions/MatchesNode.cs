namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public class MatchesNode : ConditionNode
{
    public MatchesNode(ExpressionNode l, ExpressionNode r) : base(l, r)
    {
    }
}