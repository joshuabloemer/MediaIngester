namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public class AnyNode : ConditionNode
{
    public AnyNode() : base(new EmptyNode(), new EmptyNode())
    {
    }
}