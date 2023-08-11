namespace MediaIngesterCore.Parsing.SyntaxTree;

public class ProgramNode : SyntaxNode
{
    public ProgramNode(VarBlockNode varBlock, BlockNode block)
    {
        this.VarBlock = varBlock;
        this.Block = block;
    }

    public VarBlockNode VarBlock { get; }
    public BlockNode Block { get; }
}