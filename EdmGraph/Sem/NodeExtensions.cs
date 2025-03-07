


namespace EdmGraph.Sem;


public static class NodeExtensions
{

    public static TSem? Resolve<TSyn, TSem>(this Syn.INode synNode, Func<TSyn, string> prop, TryGet tryGet)
        where TSyn : Syn.INode
        where TSem : INode
    {
        return synNode is TSyn syn && tryGet(prop(syn), out var semNode) && semNode is TSem sem ? sem : default!; // TODO: warn if fails
    }

}