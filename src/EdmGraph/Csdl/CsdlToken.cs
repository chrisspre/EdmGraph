namespace EdmGraph;

using System;
using System.Xml.Linq;

public enum CsdlTokenType
{
    StartElement,
    EndElement,
    Annotation
}


public record CsdlToken
{
    private CsdlToken(CsdlTokenType type, string name)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }

    public CsdlTokenType Type { get; }

    public record StartElement(string Name, Dictionary<string, string> Attributes, LineInfo LineInfo) : CsdlToken(CsdlTokenType.StartElement, Name)
    {

        public override string ToString()
        {
            return $"{{StartElement {Name} [{string.Join(", ", Attributes.Select(kvp => $"{kvp.Key}=\"{kvp.Value}\""))}] @ {LineInfo}}}";
        }

        internal string Get(string name)
        {
            return Attributes.TryGetValue(name, out var value) ? value : "" ; 
        }

        internal string? Get(string name, string? defaultValue)
        {
            return Attributes.TryGetValue(name, out var value) ? value : defaultValue;
        }

        internal T Parse<T>(string name, T? defaultValue = default) where T : IParsable<T>
        {
            if (Attributes.TryGetValue(name, out var value) && T.TryParse(value, null, out var parsedValue))
            {
                return parsedValue;
            }
            return defaultValue ?? default!;
        }


        // Helper methods for parsing nullable values
        internal T? ParseNullable<T>(string attributeName) where T : struct, IParsable<T>
        {
            return Attributes.TryGetValue(attributeName, out var str) && T.TryParse(str, null, out var value) ? value : null;
        }
    }

    public record EndElement(string Name) : CsdlToken(CsdlTokenType.StartElement, Name)
    {
        public override string ToString()
        {
            return $"{{EndElement {Name}}}";
        }
    }

    public record AnnotationExpression(XDocument Value) : CsdlToken(CsdlTokenType.Annotation, "")
    {
        public override string ToString()
        {
            return $"{{AnnotationExpression {Value.ToString(SaveOptions.DisableFormatting)} }}";
        }
    }
}
