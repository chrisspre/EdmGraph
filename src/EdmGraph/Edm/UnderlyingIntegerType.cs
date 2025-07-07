namespace EdmGraph.Edm;

public readonly struct UnderlyingIntegerType : IParsable<UnderlyingIntegerType>
{
    private UnderlyingIntegerType(string Name) { 
        this.Name = Name;
    }

    public override string ToString() => Name;

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out UnderlyingIntegerType result)
    {
        switch(s)
        {
            case "Edm.Byte" : result = Byte; return true;
            case "Edm.SByte" : result = SByte; return true;
            case "Edm.Int16" : result = Int16; return true;
            case "Edm.Int32" : result = Int32; return true;
            case "Edm.Int64" : result = Int64; return true;
            default: { result = default; return false; }
        };
    }

    public string Name { get;  } 

    public static UnderlyingIntegerType Parse(string value, IFormatProvider? formatProvider = null)
    {
        return TryParse(value, formatProvider, out var result)
            ? result
            : throw new FormatException($"Invalid format for UnderlyingIntegerType: {value}");  
    }

    public static UnderlyingIntegerType Byte { get; } = new UnderlyingIntegerType("Edm.Byte");
    public static UnderlyingIntegerType SByte { get; } = new UnderlyingIntegerType("Edm.SByte");
    public static UnderlyingIntegerType Int16 { get; } = new UnderlyingIntegerType("Edm.Int16");
    public static UnderlyingIntegerType Int32 { get; } = new UnderlyingIntegerType("Edm.Int32");
    public static UnderlyingIntegerType Int64 { get; } = new UnderlyingIntegerType("Edm.Int64");
}
