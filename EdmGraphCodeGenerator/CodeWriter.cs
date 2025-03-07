namespace omm.generator;

internal class CodeWriter(TextWriter writer) : IDisposable
{
    private readonly TextWriter _innerWriter = writer;

    public void WriteLine(CodeWriterInterpolatedStringHandler line)
    {
        _innerWriter.WriteLine(line.GetString());
    }

    public void WriteLine(string line)
    {
        _innerWriter.WriteLine(line);
    }

    internal void WriteLine()
    {
        _innerWriter.WriteLine();
    }

    public void Flush()
    {
        _innerWriter.Flush();
    }
    public void Dispose()
    {
        ((IDisposable)_innerWriter).Dispose();
    }

}
