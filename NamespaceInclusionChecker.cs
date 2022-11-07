using System.Collections.Generic;
using System.Linq;

namespace Brain2CPU.MvvmEssence;

public class NamespaceInclusionChecker
{
    private readonly HashSet<string> _topLevelNs = new ();
    private readonly HashSet<string> _recursiveNs = new ();

    public NamespaceInclusionChecker(IEnumerable<string> list)
    {
        foreach (var s in list)
        {
            if (s.EndsWith(".*"))
            {
                var ns = s.Remove(s.Length - 2);
                _topLevelNs.Add(ns);
                _recursiveNs.Add(ns + ".");
            }
            else
                _topLevelNs.Add(s);
        }
    }

    public bool Includes(string s)
    {
        if (string.IsNullOrEmpty(s))
            return false;

        if (_topLevelNs.Contains(s))
            return true;

        return _recursiveNs.Any(s.StartsWith);
    }
}
