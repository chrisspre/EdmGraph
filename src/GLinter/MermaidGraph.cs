namespace GLinter;

using EdmGraph.Edm;

static class MermaidGraph{


    public static void WriteGraph(DataServices root, string filePath, string title)
    {
        var stack = new Stack<(int ParentIx, IEdmElement Element)>();

        foreach (var child in root.Children)
        {
            stack.Push((0, child));
        }

        var nodes = new List<(string Id, string Label, string? Category)>();
        var index = new Dictionary<IEdmElement, int>();
        var links = new List<(string Source, string Label, string Target)>();

        var ix = 0;
        while (stack.TryPop(out var entry))
        {
            ix += 1;
            var (parentId, element) = entry;
            index[element] = ix;
            nodes.Add(($"n{ix}", $"{element.GetType().Name}{(string.IsNullOrWhiteSpace(element.Name) ? "" : $": {element.Name}")}", null));
            links.Add(($"n{parentId}", "child", $"n{ix}"));

            foreach (var child in element.Children)
            {
                stack.Push((ix, child));
            }
        }

        foreach (var element in index.Keys)
        {
            AddReferenceLinks(element, index, links);
        }


        MermaidGen.Generator.WriteGraphAsHtml(filePath, nodes, links, title);
    }

    private static void AddReferenceLinks(IEdmElement element, Dictionary<IEdmElement, int> index, List<(string Source, string Label, string Target)> links)
    {
        switch (element)
        {
            case EntityType entityType:
                if (entityType.BaseType is not null)
                {
                    links.Add(($"n{index[entityType]}", "BaseType", $"n{index[entityType.BaseType]}"));
                }
                break;

            case ComplexType complexType:
                // TODO: BaseType is a string reference, not a resolved object
                // Will need deferred resolution to create these links
                // if (complexType.BaseType is not null)
                // {
                //     links.Add(($"n{index[complexType]}", "BaseType", $"n{index[complexType.BaseType]}"));
                // }
                break;

            case Property property:
                if (property.Type is not null && property.Type is not PrimitiveType)
                {
                    links.Add(($"n{index[property]}", "Type", $"n{index[property.Type]}"));
                }
                break;

            case NavigationProperty navProperty:
                if (navProperty.Type is not null)
                {
                    links.Add(($"n{index[navProperty]}", "Type", $"n{index[navProperty.Type]}"));
                }
                break;

            case PropertyRef pref:
                // TODO: PropertyRef.Name is a string reference, not a resolved Property object
                // Will need deferred resolution to create these links
                // if (pref.Name is not null)
                // {
                //     foreach (var segment in pref.Name.Select((p, i) => (p, i)))
                //     {
                //         links.Add(($"n{index[pref]}", $"Key{segment.i + 1}", $"n{index[segment.p]}"));
                //         // TODO add link style for these key links: 
                //         // linkStyle 13,14,15,16,19 stroke - width:2px,stroke - dasharray: 7,2,3,2,7,2;
                //     }
                // }
                break;
        }
    }

}