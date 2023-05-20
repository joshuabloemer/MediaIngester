namespace MediaIngesterCore.Parsing.SyntaxTree;

public class BlockNode : SyntaxNode {
    public List<SyntaxNode> Statements { get; }
    public BlockNode(SyntaxNode node) {
        this.Statements = new List<SyntaxNode> { node };
    }

    public BlockNode Concat(BlockNode tail) {
        this.Statements.AddRange(tail.Statements);
        return(this);
    }
}