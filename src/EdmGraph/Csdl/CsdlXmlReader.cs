using System;
using System.Xml;
using System.Xml.Linq;

namespace EdmGraph;

public static class CsdlXmlReader
{

    private static readonly XNamespace EDM = "http://docs.oasis-open.org/odata/ns/edm";

    public static readonly XmlReaderSettings XmlReaderSettings = new()
    {
        IgnoreWhitespace = true,
        IgnoreComments = true,
        DtdProcessing = DtdProcessing.Prohibit,
        CloseInput = true,
        ConformanceLevel = ConformanceLevel.Fragment
    };

    public static IEnumerable<CsdlToken> ReadTokens(string path)
    {
        var reader = XmlReader.Create(path, XmlReaderSettings);
        return ReadTokens(reader);
    }

    public static IEnumerable<CsdlToken> ReadTokens(XmlReader reader)
    {
        var skip = false;
        while (skip || reader.Read())
        {
            skip = false;
            switch (reader.NodeType)
            {
                case XmlNodeType.Element when reader.NamespaceURI != EDM.NamespaceName:
                    continue;

                case XmlNodeType.EndElement when reader.NamespaceURI != EDM.NamespaceName:
                    continue;

                case XmlNodeType.Element when reader.LocalName == "Annotation":
                    var attributes = ReadAttributes(reader);
                    var lineInfo = GetLineInfo(reader);

                    // Console.WriteLine("{0} {1} {2}", reader.NodeType, reader.Name, lineInfo);
                    var innerXml = reader.ReadInnerXml();
                    // this positions the reader after the closing tag of the Annotation element. Hence the skip below.
                    skip = true;

                    // it is important to construct the expression before the StartElement since it might modify the attribute list
                    var expr = CreateAnnotationExpression(innerXml, attributes);

                    yield return new CsdlToken.StartElement("Annotation", attributes, lineInfo);
                    yield return new CsdlToken.AnnotationExpression(expr);
                    yield return new CsdlToken.EndElement("Annotation");
                    break;

                case XmlNodeType.Element:
                    lineInfo = GetLineInfo(reader);
                    yield return new CsdlToken.StartElement(reader.LocalName, ReadAttributes(reader), lineInfo);
                    if (reader.IsEmptyElement)
                    {
                        yield return new CsdlToken.EndElement(reader.LocalName);
                    }
                    break;

                case XmlNodeType.EndElement:
                    yield return new CsdlToken.EndElement(reader.LocalName);
                    break;
            }
        }

        static (ushort, ushort) GetLineInfo(XmlReader reader)
        {
            var li = (IXmlLineInfo)reader;
            return ((ushort)li.LineNumber, (ushort)li.LinePosition);
        }
    }

    private static Dictionary<string, string> ReadAttributes(XmlReader _reader)
    {
        var attributes = new Dictionary<string, string>();
        if (_reader.HasAttributes)
        {
            while (_reader.MoveToNextAttribute())
            {
                if (_reader.Name.StartsWith("xmlns")) { continue; } // skip namespace declarations
                attributes[_reader.Name] = _reader.Value;
            }
            _reader.MoveToElement();
        }

        return attributes;
    }

    private static readonly HashSet<string> AnnotationExpressionAttributes = ["String", "Int", "Bool", "Decimal", "DateTimeOffset", "Duration", "Binary", "Guid", "EnumMember", "Path"];

    private static XDocument CreateAnnotationExpression(string innerXml, Dictionary<string, string> attributes)
    {
        if (string.IsNullOrEmpty(innerXml))
        {
            foreach (var attr in attributes.Keys)
            {
                if (AnnotationExpressionAttributes.Contains(attr))
                {
                    // Console.WriteLine("Attribute Name and Value '{0}' '{1}'", EDM + attr, attributes[attr]);
                    var annotationDoc = new XDocument(new XElement(EDM + attr, new XText(attributes[attr])));

                    attributes.Remove(attr);

                    return annotationDoc;
                }
            }
            // return XDocument.Parse($"<Null xmlns=\"{EDM.NamespaceName}\"></Null>");
            return new XDocument(new XElement(EDM + "Null"));
        }
        else
        {
            return XDocument.Parse(innerXml);
        }
    }
}

