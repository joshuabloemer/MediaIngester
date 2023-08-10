using MediaIngesterCore.Parsing.SyntaxTree;

namespace MediaIngesterCore.Parsing;

public static class FileTreeEvaluator
{
    public static object Evaluate(SyntaxNode node)
    {
        switch (node)
        {
            case BlockNode b: return BlockNode(b);
            case RuleNode r: return RuleNode(r);
            case StringNode s: return s.Value;
            case PathPartNode p: return $"path[{p.Part}]";
            case MetadataNode m: return $"{m.Directory} - {m.Tag}";
        }

        throw new Exception($"Unknown node type {node.GetType()}");
    }


    private static List<string> RuleNode(RuleNode r)
    {
        List<string> result = new();

        if (r.GetIndent() is not null)
        {
            foreach (string path in (List<string>)Evaluate(r.GetIndent()))
            {
                // if (r.Path is not EmptyNode)
                //     result.Add(Path.Join((string)Evaluate((PathNode)r.Path), path));
                // else
                //     result.Add(path);
            }
        }

        // result.Add((string)Evaluate((PathNode)r.Path));
        if (r.Under is not EmptyNode) result.AddRange((List<string>)Evaluate(r.Under));

        return result;
    }

    private static List<string> BlockNode(BlockNode b)
    {
        List<string> result = new();
        foreach (RuleNode rule in b.Statements) result.AddRange((List<string>)Evaluate(rule));
        return result;
    }
}