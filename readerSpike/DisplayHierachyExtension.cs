static class DisplayHierachyExtension
{
    public static void DisplayHierachy(this TextWriter output, CsdlXmlReader reader)
    {
        var indent = "";
        while (reader.Read())
        {
            // if (reader.Current is CsdlToken.StartElement xse && xse.Name.StartsWith("edmx:") ||
            //     reader.Current is CsdlToken.EndElement xee && xee.Name.StartsWith("edmx:"))
            // {
            //     continue;
            // }
            if (reader.Current is CsdlToken.EndElement ee)
            {
                indent = indent[0..^4];
            }

            output.WriteLine("{0}{1}",
                indent,
                reader.Current.ToString().Escape());

            if (reader.Current is CsdlToken.StartElement se)
            {
                indent += "    ";
            }
        }
    }
}