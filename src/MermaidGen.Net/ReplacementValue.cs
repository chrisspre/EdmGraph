namespace MermaidGen.Net;

using System.Diagnostics.CodeAnalysis;


public readonly struct ReplacementValue
{
    // public string? StringValue { get; }
    // public Action<TextWriter>? ActionValue { get; }
    private readonly object? _value;
    public bool IsString => _value is string;
    public bool IsAction => _value is Action<TextWriter>;

    public ReplacementValue(string value) => _value = value;
    public ReplacementValue(Action<TextWriter> value) => _value = value;

    public static implicit operator ReplacementValue(string value) => new(value);
    public static implicit operator ReplacementValue(Action<TextWriter> value) => new(value);

    public void WriteTo(TextWriter writer)
    {
        switch (_value)
        {
            case string s:
                writer.Write(s);
                break;
            case Action<TextWriter> action:
                action(writer);
                break;
            default:
                throw new InvalidOperationException("Internal error TemplateValue must be either constructed form a string or an Action<TextWriter>.");
        }
    }

    public bool TryGetAction([MaybeNullWhen(false)] out Action<TextWriter> action)
    {
        if (_value is Action<TextWriter> act)
        {
            action = act;
            return true;
        }
        action = null;
        return false;
    }

    internal bool TryGetString([MaybeNullWhen(false)] out string value)
    {
        if (_value is string str)
        {
            value = str;
            return true;
        }
        value = null;
        return false;
    }
}
