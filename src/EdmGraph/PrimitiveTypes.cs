namespace EdmGraph;
using EdmGraph.Edm;

public static class PrimitiveTypes
{
    public static PrimitiveType FromName(string name)
    {
        return name switch
        {
            "Edm.String" => String,
            "Edm.Int16" => Int16,
            "Edm.Int32" => Int32,
            "Edm.Int64" => Int64,
            "Edm.Byte" => Byte,
            "Edm.Boolean" => Boolean,
            "Edm.Decimal" => Decimal,
            "Edm.Double" => Double,
            "Edm.Single" => Single,
            "Edm.DateTimeOffset" => DateTimeOffset,
            "Edm.Guid" => Guid,
            "Edm.Binary" => Binary,
            "Edm.TimeOfDay" => TimeOfDay,
            "Edm.Date" => Date,
            "Edm.Duration" => Duration,
            "Edm.Stream" => Stream,
            _ => throw new ArgumentException($"Unknown primitive type: {name}")
        };
    }

    public static readonly PrimitiveType String = new PrimitiveType("Edm.String");
    public static readonly PrimitiveType Int16 = new PrimitiveType("Edm.Int16");
    public static readonly PrimitiveType Int32 = new PrimitiveType("Edm.Int32");
    public static readonly PrimitiveType Int64 = new PrimitiveType("Edm.Int64");
    public static readonly PrimitiveType Byte = new PrimitiveType("Edm.Byte");
    public static readonly PrimitiveType Boolean = new PrimitiveType("Edm.Boolean");
    public static readonly PrimitiveType Decimal = new PrimitiveType("Edm.Decimal");
    public static readonly PrimitiveType Double = new PrimitiveType("Edm.Double");
    public static readonly PrimitiveType Single = new PrimitiveType("Edm.Single");
    public static readonly PrimitiveType Guid = new PrimitiveType("Edm.Guid");
    public static readonly PrimitiveType Binary = new PrimitiveType("Edm.Binary");
    
    public static readonly PrimitiveType DateTimeOffset = new PrimitiveType("Edm.DateTimeOffset");    
    public static readonly PrimitiveType Date = new PrimitiveType("Edm.Date"); 
    public static readonly PrimitiveType TimeOfDay = new PrimitiveType("Edm.TimeOfDay");
    public static readonly PrimitiveType Duration = new PrimitiveType("Edm.Duration");
    public static readonly PrimitiveType Stream = new PrimitiveType("Edm.Stream");
}
