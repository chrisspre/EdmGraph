namespace System.Collections.Generic;

public static class AttributesExtensions
{
    public static string Format(this IEnumerable<(string Name, object? Value)> attributes)
    {
        var strings = attributes.Select(a => a.Value == null ? a.Name : $"{a.Name}={a.Value}");
        return $"{{{string.Join(" ", strings)}}}";
    }
}