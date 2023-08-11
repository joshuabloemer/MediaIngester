using MediaIngesterCore.Parsing.SyntaxTree.Conditions;

namespace MediaIngesterCore.Parsing.SyntaxTree;

public class RuleNode : SyntaxNode
{
    public RuleNode(ConditionNode condition, SyntaxNode path, RuleNode? under, BlockNode? indent)
    {
        this.Condition = condition;
        this.Path = path;
        this.Under = under;
        this.Indent = indent;
    }

    public ConditionNode Condition { get; }
    public SyntaxNode Path { get; }
    public RuleNode? Under { get; }
    public BlockNode? Indent { get; }

    public BlockNode? GetIndent()
    {
        return this.Indent ?? this.Under?.GetIndent();
    }
}