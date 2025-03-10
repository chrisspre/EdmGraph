
using System.Collections.ObjectModel;

namespace EdmGraph.Syn;

public interface INode
{
    Sem.INode Prepare();

    INode? Parent { get; internal set; }

    IEnumerable<INode> Children { get; }

    IEnumerable<(string Name, object? Value)> Properties { get; }
}

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