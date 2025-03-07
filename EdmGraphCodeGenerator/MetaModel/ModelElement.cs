namespace omm.generator.Model;


public record ModelElement(string Name, string[] Children, Property[] Properties = null!)
{
}

public static class ModelExtensions
{
    public static void Add(this Dictionary<string, ModelElement> dictionary, ModelElement element)
    {
        dictionary.Add(element.Name, element);
    }

    public static IEnumerable<string> Parents(this Dictionary<string, ModelElement> model, string element)
    {
        return model.Where(e => e.Value.Children.Contains(element)).Select(e => e.Key);
    }
}
