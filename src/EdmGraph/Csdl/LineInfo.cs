namespace EdmGraph;

public record struct LineInfo(ushort Item1, ushort Item2)
{
    public static implicit operator (ushort, ushort)(LineInfo value)
    {
        return (value.Item1, value.Item2);
    }

    public static implicit operator LineInfo((ushort, ushort) value)
    {
        return new LineInfo(value.Item1, value.Item2);
    }
    public static LineInfo None { get; } = new LineInfo(0, 0);

    public override readonly string ToString()
    {
        return $"({Item1}, {Item2})";
    }
}