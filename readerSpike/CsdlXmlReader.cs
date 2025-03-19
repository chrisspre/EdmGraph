// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Xml;


class CsdlXmlReader(XmlReader reader) : IDisposable
{
    private readonly XmlReader reader = reader;

    public CsdlToken Current { get; private set; } = default!;

    private (string, (int, int))? selfClosing = default;

    private CsdlToken.AnnotationExpression? expression;

    const string EdmNS = "http://docs.oasis-open.org/odata/ns/edm";
    const string EdmxNS = "http://docs.oasis-open.org/odata/ns/edmx";

    private static readonly IReadOnlyDictionary<string, string> EmptyAttributesDictionary = new Dictionary<string, string>().AsReadOnly();


    public bool Read()
    {
        // reset state
        Current = default!;

        if (expression is not null)
        {
            Current = expression;
            expression = null;
            return true;
        }

        if (selfClosing is (var name, var pos))
        {
            Current = new CsdlToken.EndElement(name, pos);
            selfClosing = default;
            return true;
        }

        bool ReadNext()
        {
            if (reader.ReadState == ReadState.Initial)
            {
                return reader.ReadToDescendant("Schema", EdmNS);
            }
            else
            {
                return reader.Read();
            }
        }

        while (ReadNext())
        {
            var currentStart = reader.GetLineInfo();
            var currentName = reader.Name;

            switch (reader.NodeType)
            {
                case XmlNodeType.Element when reader.NamespaceURI is EdmNS && reader.Name is "Null" or "Apply":
                    Current = ReadExpressionToken(reader);
                    return true;

                // model element start
                case XmlNodeType.Element when reader.NamespaceURI is EdmNS:

                    if (reader.IsEmptyElement)
                    {
                        selfClosing = (currentName, currentStart);
                    }

                    if (reader.HasAttributes)
                    {
                        var p = reader.GetLineInfo();
                        var attributes = reader.GetAttributeDictionary();
                        if (currentName is "Annotation" or "PropertyValue")
                        {
                            var expr = ExtractAnnotationExpressions(attributes);
                            if (expr is not null)
                            {
                                expression = new CsdlToken.AnnotationExpression(expr, p);
                            }
                        }
                        Current = new CsdlToken.StartElement(currentName, attributes.AsReadOnly(), currentStart);
                    }
                    else
                    {
                        Current = new CsdlToken.StartElement(currentName, EmptyAttributesDictionary, currentStart);
                    }
                    return true;

                case XmlNodeType.EndElement when reader.NamespaceURI is EdmNS:
                    Current = new CsdlToken.EndElement(currentName, currentStart);
                    return true;

                case XmlNodeType.EndElement when reader.NamespaceURI is EdmxNS:
                    break; // end the switch and therefore skip and read to next

                case XmlNodeType.XmlDeclaration:
                case XmlNodeType.Whitespace:
                case XmlNodeType.Text:
                    break; // end the switch and therefore skip and read to next

                default:
                    // TODO: what to do here ?
                    Trace.WriteLine(string.Format("{0} {1}", reader.Name, reader.GetLineInfo()));
                    Current = null!;
                    return true;
            }
        }
        return false;
    }

    // read annotation expression in element notation
    // See https://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html#sec_AnnotationElement
    private CsdlToken.AnnotationExpression ReadExpressionToken(XmlReader reader)
    {
        var lineInfo = reader.GetLineInfo();
        AnnotationExpression expr = ReadExpression(reader);
        return new CsdlToken.AnnotationExpression(expr, lineInfo);
    }

    private AnnotationExpression ReadExpression(XmlReader reader)
    {
        var name = reader.Name;

        switch (name)
        {
            case "Null":
                return new AnnotationExpression.Null();

            case "Path":
                return new AnnotationExpression.Path(reader.ReadElementContentAsString());

            case "String":
                return new AnnotationExpression.String(reader.ReadElementContentAsString());

            case "Apply":
                var apply = new AnnotationExpression.Apply(reader.GetAttribute("Function")!);
                var d = reader.Depth;
                while (reader.Read() && reader.Depth > d)
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.NamespaceURI == EdmNS)
                    {
                        apply.Arguments.Add(ReadExpression(reader));
                    }
                }
                return apply;
            default:
                throw new InvalidOperationException($"Unexpected expression {name}");
        }
    }

    private static readonly Dictionary<string, Func<string, AnnotationExpression>> AnnotationAttributes = new()
    {
        {"String", v => new AnnotationExpression.String(v)},
        {"Bool",   v => new AnnotationExpression.Bool(bool.Parse(v))},
        {"Int",    v => new AnnotationExpression.Int(int.Parse(v))},
        {"Path",   v => new AnnotationExpression.Path(v)},
        // {"EnumMember", v => new AnnotationExpression.EnumMember(v)},
        // {"Float",  v => new AnnotationExpression.Float(float.Parse(v))},
        // {"Guid",   v => new AnnotationExpression.Guid(Guid.Parse(v))},
        // {"Date",   v => new AnnotationExpression.Date(DateTime.Parse(v))},
        // {"DateTimeOffset", v => new AnnotationExpression.DateTimeOffset(DateTimeOffset.Parse(v))},
        // {"Duration", v => new AnnotationExpression.Duration(TimeSpan.Parse(v))},
        // {"TimeOfDay", v => new AnnotationExpression.TimeOfDay(TimeSpan.Parse(v))},
        // {"Decimal", v => new AnnotationExpression.Decimal(decimal.Parse(v))},
        // {"Binary", v => new AnnotationExpression.Binary(Convert.FromBase64String(v))},
    };

    /// <summary>
    /// extract the xml attributes that represent annotation exptessions in attribute notation
    /// see for example https://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html#sec_Decimal
    /// </summary>
    /// <param name="attribs"></param>
    /// <returns></returns>.
    private static AnnotationExpression? ExtractAnnotationExpressions(Dictionary<string, string> attribs)
    {
        foreach (var (name, factory) in AnnotationAttributes)
        {
            if (attribs.TryGetValue(name, out var value))
            {
                var expr = factory(value);
                attribs.Remove(name);
                return expr;
            }
        }

        return null;
    }

    public void Dispose()
    {
        ((IDisposable)reader).Dispose();
    }
}
