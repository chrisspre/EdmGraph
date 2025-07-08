namespace EdmGraph;

public interface IEdmElementFactory<T> where T : IEdmElement
{
    static abstract T Create(CsdlToken.StartElement startElement, IEdmElement parent, DeferredActionRegistry actions);
}
