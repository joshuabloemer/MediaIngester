using MediaIngesterCore.Parsing.SyntaxTree;
using MediaIngesterCore.Parsing.SyntaxTree.Conditions;
using MediaIngesterCore.Parsing.SyntaxTree.Conditions.Types;
using MetadataExtractor;
using static MediaIngesterCore.Utils;

namespace MediaIngesterCore.Parsing
{
    internal class Evaluator
    {
        public string FilePath { get; }

        public Dictionary<String, Dictionary<String, String>> Metadata { get; } = new Dictionary<String, Dictionary<String, String>>();

        public DateTime DateTaken { get; }

        public Boolean RuleMatched { get; private set; } = false;

        public Evaluator(string filePath)
        {
            this.FilePath = filePath;
            this.DateTaken = GetDateTaken(filePath);

            try
            {
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
                foreach (var directory in directories)
                {
                    Metadata[directory.Name] = new Dictionary<String, String>();
                    foreach (var tag in directory.Tags)
                    {
                        Metadata[directory.Name][tag.Name] = tag.Description!;
                    }
                }
            }
            catch (ImageProcessingException) { }
        }

        public object Evaluate(SyntaxNode node)
        {
            switch (node)
            {
                case ProgramNode p: return Evaluate(p.Block);
                case StringNode n: return n.Value;
                case BlockNode b: return Block(b);
                case PathNode p: return Path(p);
                case RuleNode r: return Rule(r);
                case AnyNode: return true;
                case EqualsNode e: return Equals(e);
                case NotNode n: return Not(n);
                case LessThanNode l: return Less(l);
                case GreaterThanNode g: return Greater(g);
                case LessOrEqualNode l: return LessOrEqual(l);
                case GreaterOrEqualNode g: return GreaterOrEqual(g);
                case ContainsNode c: return ContainsNode(c);
                case MetadataNode m: return MetadataNode(m);
                case ExtensionNode: return System.IO.Path.GetExtension(this.FilePath).Remove(0, 1);
                case YearNode: return this.DateTaken.Year.ToString();
                case MonthNode: return this.DateTaken.Month.ToString("d2");
                case DayNode: return this.DateTaken.Day.ToString("d2");
                case HourNode: return this.DateTaken.Hour.ToString("d2");
                case MinuteNode: return this.DateTaken.Minute.ToString("d2");
                case SecondNode: return this.DateTaken.Second.ToString("d2");
                case PathPartNode p: return PathPartNode(p);
                case FileNameNode: return System.IO.Path.GetFileNameWithoutExtension(this.FilePath);
                case PathNameNode: return this.FilePath;
                case EmptyNode: return null;
            }
            throw (new Exception($"Unknown node type {node.GetType()}"));
        }

        private object ContainsNode(ContainsNode c)
        {
            string lhs = (string)Evaluate(c.l);
            string rhs = (string)Evaluate(c.r);
            return lhs.Contains(rhs);
        }

        private object PathPartNode(PathPartNode p)
        {
            string[] pathParts = this.FilePath.Split("\\");
            if (p.Part >= 0)
                return pathParts[p.Part];
            else
                return pathParts[^Math.Abs(p.Part)];
        }

        private object MetadataNode(MetadataNode m)
        {

            Dictionary<String, String> directory;
            String tag;
            if (this.Metadata.TryGetValue(m.Directory, out directory))
            {
                if (directory.TryGetValue(m.Tag, out tag))
                {
                    return tag;
                }
            }
            return "null";
        }

        private object GreaterOrEqual(GreaterOrEqualNode g)
        {
            return Convert.ToDecimal(Evaluate(g.l)) >= Convert.ToDecimal(Evaluate(g.r));
        }

        private object LessOrEqual(LessOrEqualNode l)
        {
            return Convert.ToDecimal(Evaluate(l.l)) <= Convert.ToDecimal(Evaluate(l.r));
        }

        private object Greater(GreaterThanNode g)
        {
            return Convert.ToDecimal(Evaluate(g.l)) > Convert.ToDecimal(Evaluate(g.r));
        }

        private object Less(LessThanNode l)
        {
            return Convert.ToDecimal(Evaluate(l.l)) < Convert.ToDecimal(Evaluate(l.r));
        }

        private object Not(NotNode n)
        {
            return Convert.ToString(Evaluate(n.l)) != Convert.ToString(Evaluate(n.r));
        }

        private object Equals(EqualsNode e)
        {
            return Convert.ToString(Evaluate(e.l)) == Convert.ToString(Evaluate(e.r));
        }

        private string? Rule(RuleNode r)
        {
            string? result = null;
            if ((bool)Evaluate(r.Condition))
            {
                result = (string)Evaluate(r.Path);
                this.RuleMatched = true;
                SyntaxNode indent = r.GetIndent();
                if (indent is not null)
                {
                    result = System.IO.Path.Join(result, Convert.ToString(Evaluate(indent)));
                }
            }
            else if (r.Under is not EmptyNode)
            {
                result = (string)(Evaluate(r.Under));
            }
            return result;
        }

        private string Path(PathNode p)
        {
            string result = "";
            foreach (SyntaxNode part in p.Parts)
            {
                result = System.IO.Path.Join(result, Convert.ToString(Evaluate(part)));
            }
            return result;
        }

        private string? Block(BlockNode b)
        {
            string? result = null;
            foreach (var statement in b.Statements)
            {
                result = (string)Evaluate(statement);   // use cast instead of Convert.ToString to allow for null return
                if (result is not null) return result;
            }
            return null;
        }
    }
}