namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public abstract class ConditionNode : SyntaxNode
{
    public ConditionNode(SyntaxNode l, SyntaxNode r)
    {
        this.L = l;
        this.R = r;
    }

    public SyntaxNode? L { get; }
    public SyntaxNode? R { get; }
}