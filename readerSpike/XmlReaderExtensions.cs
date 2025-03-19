// See https://aka.ms/new-console-template for more information
using System.Xml;

public static class XmlReaderExtensions
{
    public static (int, int) GetLineInfo(this XmlReader reader)
    {
        var info = (IXmlLineInfo)reader;
        return (info.LineNumber, info.LinePosition);
    }

    // public static Dictionary<string, (string, (int, int))> GetAttributeDictionary(this XmlReader reader)
    public static Dictionary<string, string> GetAttributeDictionary(this XmlReader reader)
    {
        var attributes = new Dictionary<string, string>();
        for (var i = 0; i < reader.AttributeCount; i++)
        {
            reader.MoveToAttribute(i);
            if (reader.Name != "xmlns")
            {
                attributes.Add(reader.Name, reader.Value);
            }
        }
        return attributes;
    }

    public static void SkipToMatchingEndElement(this XmlReader reader)
    {

        string startElementName = reader.Name;
        int depth = reader.Depth;

        while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == startElementName && reader.Depth == depth))
        {
            // Continue reading until the matching end element is found
        }

        // Now the reader is positioned at the end element
    }
}
