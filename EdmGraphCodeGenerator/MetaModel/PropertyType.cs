namespace omm.generator.Model;

/// <summary>
/// A discriminated union representing the type 
/// of a (meta model) property
///     PropertyType
///         = Primitive(PrimitiveType Type)
///         | Reference(string[] Targets)
///         | Intrinsic(string Name)
///         | Path(string[] Path)
/// </summary>
public abstract record class PropertyType
{
    private PropertyType() { }

    public sealed record Primitive(PrimitiveType Type) : PropertyType;

    public enum PrimitiveType
    {
        String, Bool, Int,
    }

    public static Primitive String { get; } = new Primitive(PrimitiveType.String);

    public static Primitive Boolean { get; } = new Primitive(PrimitiveType.Bool);

    public static Primitive Int { get; } = new Primitive(PrimitiveType.Int);

    public sealed record Intrinsic(string Name) : PropertyType;

    public sealed record Reference(params string[] Targets) : PropertyType
    {
        override public string ToString() => $"{{Reference Targets = [ {string.Join(", ", Targets)} ]}}";
    }

    public static PropertyType CreateReference(params string[] types) => new Reference(types);

}

