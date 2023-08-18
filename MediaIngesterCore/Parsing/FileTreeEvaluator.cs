using MediaIngesterCore.Parsing.SyntaxTree;
using Spectre.Console;

namespace MediaIngesterCore.Parsing;

public static class FileTreeEvaluator
{
    private static void AddToChildren(TreeNode parent, List<TreeNode> children)
    {
        foreach (TreeNode node in parent.Nodes) AddToChildren(node, children);
        if (parent.Nodes.Count == 0) parent.AddNodes(children);
    }

    public static void MergeDuplicates(IHasTreeNodes node)
    {
    }

    public static List<string> Evaluate(ProgramNode node)
    {
        return Evaluate(node.Block);
    }

    private static List<string> Evaluate(BlockNode block)
    {
        List<string> nodes = new();
        foreach (RuleNode rule in block.Statements) nodes.AddRange(Evaluate(rule));

        return nodes;
    }

    private static List<string> Evaluate(RuleNode rule)
    {
        List<string> result = new();
        switch (rule.Path)
        {
            case ExpressionNode e:

                string path = Evaluate(e);
                BlockNode? indent = rule.GetIndent();

                if (indent is null)
                {
                    result.Add(path);
                    break;
                }

                List<string> nodes = Evaluate(indent);
                result.AddRange(nodes.Select(node => Path.Join(path, node)));
                break;
            case IgnoreNode:
                break;
            default:
                throw new ArgumentOutOfRangeException(rule.Path.ToString());
        }

        if (rule.Under is not null) result.AddRange(Evaluate(rule.Under));

        return result;
    }

    private static string Evaluate(ExpressionNode expression)
    {
        return expression switch
        {
            PathPartNode p => Evaluate(p),
            MetadataNode m => Evaluate(m),
            LookupNode l => Evaluate(l),
            ValueNode v => Evaluate(v),
            _ => throw new ArgumentOutOfRangeException(expression.ToString())
        };
    }

    private static string Evaluate(ValueNode valueNode)
    {
        return valueNode.Value.Aggregate("", (current, node) => Path.Join(current, node switch
        {
            LookupNode l => Evaluate(l),
            LiteralNode l => Evaluate(l),
            _ => throw new ArgumentOutOfRangeException(node.ToString())
        }));
    }

    private static string Evaluate(LiteralNode literalNode)
    {
        return literalNode.Value;
    }

    private static string Evaluate(LookupNode lookupNode)
    {
        return $"{{{lookupNode.Name}}}";
    }

    private static string Evaluate(MetadataNode metadataNode)
    {
        return $"{{{metadataNode.Directory}:{metadataNode.Tag}:}}";
    }

    private static string Evaluate(PathPartNode pathPartNode)
    {
        return $"{{path[{pathPartNode.Part}]}}";
    }
}