namespace  EdmGraph;

using Serilog;

public class Operation
{
    public static OperationTimer Begin(string name)
    {
        return new OperationTimer(name);
    }

    public class OperationTimer
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _name;

        public OperationTimer(string name)
        {
            _name = name;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Complete()
        {
            _stopwatch.Stop();
            Log.Information(@"Operation ""{Name}"" completed in {ElapsedMilliseconds} ms", _name, _stopwatch.ElapsedMilliseconds);
        }
    }

}