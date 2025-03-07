namespace EdmGraph;

using System.Diagnostics.CodeAnalysis;

public static class SemanticModelBuilder
{
    public static Sem.Model BuildSemanticModel(this Syn.Model model)
    {
        var nameTable = BuildNameTable(model, out var semanticModel);

        bool TryGetValue(string key, [MaybeNullWhen(false)] out Sem.INode value)
        {
            value = default;
            return nameTable.TryGetValue(key, out var pair) && (value = pair.Item2) is not null;
        }

        foreach (var (syn, sem) in nameTable.Values)
        {
            (sem as Sem.INode).UpdateReferences(syn, TryGetValue);
        }

        return semanticModel;
    }

    public static Dictionary<string, (Syn.INode, Sem.INode)> BuildNameTable(Syn.Model model, out Sem.Model semanticModel)
    {
        var nameTable = new Dictionary<string, (Syn.INode, Sem.INode)>();

        semanticModel = (Sem.Model)(model as Syn.INode).Prepare();
        foreach (var child in model.Children)
        {
            var semChild = AppendToTable(child, "", nameTable);
            (semanticModel as Sem.INode).AddChild(semChild);
        }

        return nameTable;
    }

    private static Sem.INode AppendToTable(Syn.INode node, string path, Dictionary<string, (Syn.INode, Sem.INode)> nameTable)
    {
        var nodePath = path + node switch
        {
            Syn.Schema s => s.Namespace,
            Syn.EnumType e => "." + e.Name,
            Syn.ComplexType c => "." + c.Name,
            Syn.Property p => "/" + p.Name,
            Syn.Member m => "/" + m.Name,
            Syn.Term t => "@" + t.Name + (t.Qualifier == null ? "" : $"#{t.Qualifier}"),
            _ => throw new NotImplementedException($"Unknown element type {node.GetType().Name}")
        };
        var sem = (node as Syn.INode).Prepare();
        nameTable.Add(nodePath, (node, sem));
        foreach (var child in node.Children)
        {
            var semChild = AppendToTable(child, path, nameTable);
            sem.AddChild(semChild);
        }
        return sem;
    }
}


