
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace EdmGraph.Sem;

public interface INode
{
    INode? Parent { get; internal set; }

    IEnumerable<INode> Children { get; }

    IEnumerable<(string Name, object? Value)> Properties { get; }

    void AddChild(INode node);

    void UpdateReferences(Syn.INode syn, TryGet tryGet);
}

public delegate bool TryGet(string qualifiedName, [MaybeNullWhen(false)] out INode sem);


public class ChildElementCollection<TChild>(INode owner) : Collection<TChild>
    where TChild : INode
{
    public INode Owner { get; } = owner;

    public new TChild Add(TChild child)
    {
        base.Add(child);
        child.Parent = Owner;
        return child;
    }
}