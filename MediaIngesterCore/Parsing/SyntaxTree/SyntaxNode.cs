using System.Collections;
using System.Reflection;
using System.Text;

namespace MediaIngesterCore.Parsing.SyntaxTree;

public abstract class SyntaxNode
{
    // Code to render syntax trees as ASCII trees
    public override string ToString()
    {
        return this.GetType().Name.Replace("Node", "").ToLowerInvariant() + ":";
    }

    public string PrettyPrint()
    {
        StringBuilder sb = new();
        this.Render(sb);
        return sb.ToString();
    }

    public virtual void Render(StringBuilder sb, string padding = "")
    {
        sb.Append(padding).AppendLine(this.ToString());
        IEnumerable<PropertyInfo> listProperties = this.GetType().GetProperties()
            .Where(p => typeof(IList).IsAssignableFrom(p.PropertyType));
        foreach (PropertyInfo prop in listProperties)
        {
            IList? list = (IList)prop.GetValue(this);
            foreach (object? item in list)
                if (item is SyntaxNode)
                    ((SyntaxNode)item).Render(sb, padding + "  ");
        }

        IEnumerable<PropertyInfo> nodeProperties = this.GetType().GetProperties()
            .Where(p => typeof(SyntaxNode).IsAssignableFrom(p.PropertyType));
        foreach (PropertyInfo prop in nodeProperties) ((SyntaxNode)prop.GetValue(this)).Render(sb, padding + "  ");
    }
}