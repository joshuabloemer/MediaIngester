namespace MediaIngesterCore.Parsing.SyntaxTree;

public class VarBlockNode : SyntaxNode
{
    public VarBlockNode(SyntaxNode node)
    {
        this.Statements = new List<SyntaxNode> { node };
    }

    public List<SyntaxNode> Statements { get; }

    public VarBlockNode Concat(VarBlockNode tail)
    {
        this.Statements.AddRange(tail.Statements);
        return this;
    }
}