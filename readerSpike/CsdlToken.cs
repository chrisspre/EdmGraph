using System.Text;


/// <summary>
/// Represents a token returned by the csdl xml reader.
/// one of three variants: start element, end element, or annotation expression.
/// </summary>
public abstract record CsdlToken((int, int) LineInfo)
{

    public record StartElement(string Name, IReadOnlyDictionary<string, string> Attributes, (int, int) LineInfo) : CsdlToken(LineInfo)
    {


        protected override bool PrintMembers(StringBuilder builder)
        {
            builder.AppendFormat("Name: {0}", Name);
            if (Attributes.Count > 0)
            {
                if (builder.Length > 0) builder.Append(", ");
                builder.Append("Attributes: [");
                builder.AppendJoin(", ", from p in Attributes select $"{p.Key}: \"{p.Value.Escape()}\"");
                builder.Append(']');
            }
            if (builder.Length > 0) builder.Append(", ");
            builder.AppendFormat("LineInfo: {0}", LineInfo);
            return true;
        }
    }

    public record EndElement(string Name, (int, int) LineInfo) : CsdlToken(LineInfo);

    public record AnnotationExpression(global::AnnotationExpression Expression, (int, int) LineInfo) : CsdlToken(LineInfo);
}
