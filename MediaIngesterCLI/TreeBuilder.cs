namespace MediaIngesterCLI;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Summary description for CodeComparisonToolDirBuilder
/// </summary>
public class TreeBuilder
{
    public string Name { get; private set; }
    public TreeBuilder Parent { get; set; }
    public List<TreeBuilder> Children { get; private set; }

    public TreeBuilder(string name, TreeBuilder parent)
    {
        Name = name;
        Parent = parent;
        Children = new List<TreeBuilder>();
    }
    

    public override string ToString()
    {
        return Name;
    }

    public void PrintTree(StringBuilder output, bool isRoot)
    {
        string prefix;
        string pre_0 = "    ";
        string pre_1 = "│   ";
        string pre_2 = "├── ";
        string pre_3 = "└── ";

        TreeBuilder tree = this;

        if (tree.Parent != null && !(tree.Equals(tree.Parent.Children.Last())))
        {
            prefix = pre_2;
        }
        else
        {
            prefix = pre_3;
        }

        while (tree.Parent != null && tree.Parent.Parent != null)
        {
            if (tree.Parent != null && !(tree.Parent.Equals(tree.Parent.Parent.Children.Last())))
            {
                prefix = pre_1 + prefix;
            }
            else
            {
                prefix = pre_0 + prefix;
            }

            tree = tree.Parent;
        }

        if (isRoot)
        {
            output.AppendLine(this.Name);
        }
        else
        {
            output.AppendLine(prefix + this.Name);
        }

        foreach (TreeBuilder child in this.Children)
        {
            child.PrintTree(output, false);
        }
    }
    public static void TreeStruct(TreeBuilder parent, List<string> edges)
    {
        if (edges.Count == 0) return;


        List<TreeBuilder> matchedChildren = new List<TreeBuilder>();

        foreach (TreeBuilder tree in parent.Children)
        {
            if (tree.Name == edges[0])
            {
                matchedChildren.Add(tree);
            }
        }

        TreeBuilder pointer;

        if (matchedChildren.Count != 0)
        {
            pointer = matchedChildren[0];
        }
        else
        {
            pointer = new TreeBuilder(edges[0], parent);
            parent.Children.Add(pointer);
        }

        edges.RemoveAt(0);
        TreeStruct(pointer, edges);
    }

}