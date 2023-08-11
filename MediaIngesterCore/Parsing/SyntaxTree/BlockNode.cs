namespace MediaIngesterCore.Parsing.SyntaxTree;

public class BlockNode : SyntaxNode
{
    public BlockNode(RuleNode node)
    {
        this.Statements = new List<RuleNode> { node };
    }

    public List<RuleNode> Statements { get; }

    public BlockNode Concat(BlockNode tail)
    {
        this.Statements.AddRange(tail.Statements);
        return this;
    }
}