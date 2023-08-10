namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public class NotEqualsNode : ConditionNode
{
    public NotEqualsNode(SyntaxNode l, SyntaxNode r) : base(l, r)
    {
    }
}