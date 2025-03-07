namespace omm.generator;
using System.Runtime.CompilerServices;
using System.Text;

[InterpolatedStringHandler]
public readonly struct CodeWriterInterpolatedStringHandler(int literalLength, int formattedCount)
{
    private readonly StringBuilder _builder = new StringBuilder(literalLength);
    private readonly int _formattedCount = formattedCount;

    public void AppendLiteral(string s)
    {
        _builder.Append(s);
    }


    // https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings#SectionSeparator
    public void AppendFormatted<TItem>(IReadOnlyCollection<TItem> items, string format)
    {
        switch (format.Split('|'))
        {
            case [var o, var s, var c]:
                if (items.Count > 0) { _builder.Append(o); }
                _builder.AppendJoin(s, items);
                if (items.Count > 0) { _builder.Append(c); }
                break;
            case [var o, var s]:
                if (items.Count > 0) { _builder.Append(o); }
                _builder.AppendJoin(s, items);
                break;
            case [var s]:
                _builder.AppendJoin(s.Trim('\''), items);
                break;
            default:
                _builder.AppendJoin(format, items);
                break;
        }
    }

    public void AppendFormatted<T>(T t, string format) where T : IFormattable
    {
        _builder.Append(t?.ToString(format, null));
    }

    public void AppendFormatted<T>(T t)
    {
        _builder.Append(t?.ToString());
    }

    public string? GetString() => _builder.ToString();
}