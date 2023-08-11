namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public class NotNode : ConditionNode
{
    public NotNode(ConditionNode condition) : base(null, null)
    {
        this.Condition = condition;
    }

    public ConditionNode Condition { get; }
}