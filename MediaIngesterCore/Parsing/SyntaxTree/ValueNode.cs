namespace MediaIngesterCore.Parsing.SyntaxTree;

public class ValueNode : SyntaxNode
{
    public ValueNode(List<SyntaxNode> value)
    {
        this.Value = value;
    }

    public List<SyntaxNode> Value { get; }
    //
    // public override string ToString()
    // {
    //     return "value: \r\n" + string.Join("\r\n", this.Value.Select(x => x.ToString()));
    // }
}