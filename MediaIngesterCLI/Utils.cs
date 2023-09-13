using Spectre.Console;

namespace MediaIngesterCLI;

public static class Utils
{
    public static void CreateTreeRecursive(List<List<string>> paths, IHasTreeNodes node)
    {
        List<List<string>> alreadyChecked = new();
        foreach (List<string> path in paths)
        {
            if (path.Count == 0) continue;

            if (alreadyChecked.Contains(path)) continue;

            IHasTreeNodes newNode = node.AddNode(path[0].EscapeMarkup());
            List<List<string>> newList = new();
            foreach (List<string> path2 in paths)
            {
                if (path2.Count == 0) continue;
                if (!path.Equals(path2) && path2[0] == path[0])
                {
                    newList.Add(path2);
                    path2.RemoveAt(0);
                    alreadyChecked.Add(path2);
                }
            }

            // alreadyChecked.Add(path);
            newList.Add(path);

            path.RemoveAt(0);
            CreateTreeRecursive(newList, newNode);
        }
    }
}