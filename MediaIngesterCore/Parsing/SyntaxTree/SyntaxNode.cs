using System.Collections;
using System.Text;

namespace MediaIngesterCore.Parsing.SyntaxTree;

public abstract class SyntaxNode {

    // Code to render syntax trees as ASCII trees
    public override string ToString() {
        return (this.GetType().Name.Replace("Node", "").ToLowerInvariant() + ":");
    }

    public string PrettyPrint() {
        var sb = new StringBuilder();
        this.Render(sb);
        return (sb.ToString());
    }

    public virtual void Render(StringBuilder sb, string padding = "") {
        sb.Append(padding).AppendLine(this.ToString());
        var listProperties = GetType().GetProperties()
            .Where(p => typeof(IList).IsAssignableFrom(p.PropertyType));
        foreach (var prop in listProperties) {
            var list = (IList)prop.GetValue(this);
            foreach (var item in list) {
                if (item is SyntaxNode) ((SyntaxNode)item).Render(sb, padding + "  ");
            }
        }
        var nodeProperties = GetType().GetProperties().Where(p => typeof(SyntaxNode).IsAssignableFrom(p.PropertyType));
        foreach (var prop in nodeProperties) ((SyntaxNode)prop.GetValue(this)).Render(sb, padding + "  ");
    }
}