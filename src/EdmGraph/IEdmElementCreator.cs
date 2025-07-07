namespace EdmGraph;

public interface IEdmElementCreator<T> where T : IEdmElement
{
    static abstract T Create(CsdlToken.StartElement startElement, Stack<IEdmElement> stack, DeferredActionRegistry actions);
}
