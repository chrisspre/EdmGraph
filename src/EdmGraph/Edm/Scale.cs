using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace EdmGraph.Edm;

public readonly struct Scale : IParsable<Scale>
{
    public enum ScaleKind { Numeric, Floating, Variable } // the order ensures that the default is Numeric(0)

    public int Value { get; }
    public ScaleKind Kind { get; }

    private Scale(ScaleKind kind, int value)
    {
        Kind = kind;
        Value = value;
    }

    public static Scale Floating { get; } = new Scale(ScaleKind.Floating, -1);
    public static Scale Variable { get; } = new Scale(ScaleKind.Variable, -1);
    public static Scale Numeric(int value) => new Scale(ScaleKind.Numeric, value);

    public Scale(int value) : this(ScaleKind.Numeric, value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
        }
    }

    public static Scale Parse(string s, IFormatProvider? provider)
    {
        return TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"Invalid format for Scale: {s}");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Scale result)
    {
        switch (s?.ToLowerInvariant())
        {
            case "floating":
                result = Floating; // Default value for floating scale
                return true;
            case "variable":
                result = Variable; // Default value for variable scale
                return true;
            default:
                if (int.TryParse(s, out var value) && value >= 0)
                {
                    result = new Scale(ScaleKind.Numeric, value);
                    return true;
                }
                result = default;
                return false;
        }
    }

    public override string ToString()
    {
        return Kind switch
        {
            ScaleKind.Numeric => Value.ToString(),
            ScaleKind.Floating => "floating",
            ScaleKind.Variable => "variable",
            _ => throw new InvalidOperationException("Unknown scale kind")
        };
    }
}

