namespace MediaIngesterCore.Parsing.SyntaxTree;

public class ProgramNode : SyntaxNode
{
    public ProgramNode(VarBlockNode varBlock, SyntaxNode block)
    {
        this.VarBlock = varBlock;
        this.Block = block;
    }

    public VarBlockNode VarBlock { get; }
    public SyntaxNode Block { get; }
}