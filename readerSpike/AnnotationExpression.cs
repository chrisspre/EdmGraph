


public abstract record AnnotationExpression
{
    protected abstract IEnumerable<(string, object?)> Props { get; }

    public record String(string Value) : AnnotationExpression
    {
        protected override IEnumerable<(string, object?)> Props => [(nameof(Value), Value.Escape())];
    }

    public record Int(int Value) : AnnotationExpression
    {
        protected override IEnumerable<(string, object?)> Props => [(nameof(Value), Value)];
    }

    public record Bool(bool Value) : AnnotationExpression
    {
        protected override IEnumerable<(string, object?)> Props => [(nameof(Value), Value)];
    }

    public record Path(string Value) : AnnotationExpression
    {
        protected override IEnumerable<(string, object?)> Props => [(nameof(Value), Value)];
    }

    public record Null : AnnotationExpression
    {
        protected override IEnumerable<(string, object?)> Props => [];
    }

    public record Apply(string Function) : AnnotationExpression
    {
        public List<AnnotationExpression> Arguments { get; } = [];

        protected override IEnumerable<(string, object?)> Props => [(nameof(Function), Function), (nameof(Arguments), Arguments)];

        public override string ToString() => base.ToString();
    }

    public override string ToString()
    {
        var props = from p in Props select $"{p.Item1}={Format(p.Item2)}";
        return $"{{Expression Type={GetType().Name} {string.Join(" ", props)} }}";

        static string Format(object obj)
        {
            return obj switch
            {
                List<AnnotationExpression> lst => $"[{string.Join(", ", lst)}]",
                string str => $"\"{str}\"",
                _ => obj?.ToString() ?? ""
            };
        }
    }
}

