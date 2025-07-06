namespace EdmGraph.Edm;

public interface IEdmElementCreator
{
    static abstract void Create(CsdlToken.StartElement startElement, Stack<EdmElement> stack, DeferredActionRegistry actions);
}
