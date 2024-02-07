namespace MediaIngesterCore.Parsing.SyntaxTree;

public class VarBlockNode : SyntaxNode
{
    public VarBlockNode(AssignNode node)
    {
        this.Statements = new List<AssignNode> { node };
    }

    public List<AssignNode> Statements { get; }

    public VarBlockNode Concat(VarBlockNode tail)
    {
        this.Statements.AddRange(tail.Statements);
        return this;
    }
}