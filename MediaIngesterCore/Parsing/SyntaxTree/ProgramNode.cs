namespace MediaIngesterCore.Parsing.SyntaxTree;

public class ProgramNode : SyntaxNode{
    public SyntaxNode Block {get;}

    public ProgramNode(SyntaxNode block){
        this.Block = block;
    }
}