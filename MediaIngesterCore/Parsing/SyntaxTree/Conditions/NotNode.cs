namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public class NotNode : SyntaxNode
{
    public NotNode(ConditionNode condition)
    {
        this.Condition = condition;
    }

    public ConditionNode Condition { get; }
}