namespace MediaIngesterCore.Parsing.SyntaxTree.Conditions;

public class NotNode : ConditionNode{
    
    public NotNode(SyntaxNode l, SyntaxNode r):base(l,r){}
}