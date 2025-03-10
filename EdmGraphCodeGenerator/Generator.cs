
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using omm.generator;
using omm.generator.Model;

namespace omm;

public static class Generator
{
    public static void Generate(Dictionary<string, ModelElement> model, TextWriter outputWriter, bool syntactial = true)
    {
        using var writer = new CodeWriter(outputWriter);

        writer.WriteLine(Header);
        writer.WriteLine("#nullable enable");
        writer.WriteLine();
        writer.WriteLine($"namespace EdmGraph.{(syntactial ? "Syn" : "Sem")};");

        var child2Parent = model.Values
            .SelectMany(e => e.Children.Select(c => (Child: model[c].Name, Parent: $"I{e.Name}Child")))
            .GroupBy(e => e.Child, e => e.Parent)
            .ToDictionary(g => g.Key, g => g.ToArray());


        var parents2Child = model.Values
            .SelectMany(e => e.Children.Select(c => (Child: model[c].Name, Parent: e.Name)))
            .GroupBy(e => e.Parent, e => e.Child)
            .ToDictionary(g => g.Key, g => (InterfaceName: $"I{g.Key}Child", Targets: g.ToArray()));

        var referencePropertyInterfaces =
            from element in model.Values
            from property in element.Properties
                .Where(p => p.Type is PropertyType.Reference r)
                .Where(p => ((PropertyType.Reference)p.Type).Targets.Length > 1)
            select (Element: element.Name, Property: property.Name, Targets: ((PropertyType.Reference)property.Type).Targets);

        var referencePropertyInterfaceLookup = referencePropertyInterfaces
            .SelectMany(tuple => tuple.Targets.Select(target => (target, Interface: $"I{tuple.Element}{tuple.Property}")))
            .GroupBy(tuple => tuple.target, tuple => tuple.Interface)
            .ToDictionary(grp => grp.Key, grp => grp.ToList());


        // generate the interfaces for the children of model elements
        writer.WriteLine();
        writer.WriteLine("#region child  element interfaces");
        foreach (var (key, elements) in parents2Child)
        {
            writer.WriteLine();
            writer.WriteLine($$"""
            /// <summary>
            /// interface for all children of {{key}}.
            /// implemented by {{string.Join(", ", elements.Item2.Select(e => $"<see cref=\"{e}\"/>"))}}.
            /// </summary>
            """);
            writer.WriteLine($"public interface {elements.Item1} : INode {{}}");
        }
        writer.WriteLine("#endregion child  element interfaces");


        // generate the interfaces for reference properties
        {


            foreach (var (type, prop, targets) in referencePropertyInterfaces)
            {
                writer.WriteLine();
                writer.WriteLine($"/// <summary>");
                writer.WriteLine($"/// Interface for model elements that can be referenced by {type}.{prop}");
                writer.WriteLine($"/// Implemented by {string.Join(", ", targets)}");
                writer.WriteLine($"/// </summary>");
                writer.WriteLine($"public interface I{type}{prop} : INode {{");

                var commonProperties = targets
                     .Select(target => model[target].Properties.Where(p => p.Type is PropertyType.Primitive).AsEnumerable())
                     .Aggregate((prev, next) => prev.Intersect(next).ToList());

                foreach (var common in commonProperties)
                {
                    switch (common.Type)
                    {
                        case PropertyType.Primitive primitive:
                            writer.WriteLine($"    {primitive.Type} {common.Name} {{ get; }}");
                            break;
                    }
                }
                writer.WriteLine($"}}");
            }
        }

        // generate the model element classes
        foreach (var (key, node) in model)
        {
            var implements = child2Parent.TryGetValue(node.Name, out var val) ? val : [];

            writer.WriteLine();
            if (!syntactial && referencePropertyInterfaceLookup.TryGetValue(node.Name, out var targets))
            {
                implements = [.. implements, .. targets];
            }

            writer.WriteLine($"public class {node.Name} : INode{implements:, |, |}");
            writer.WriteLine($"{{");

            // constructor
            writer.WriteLine($"    public {node.Name}() => Children = new(this);");

            #region ChildElementCollection
            parents2Child.TryGetValue(node.Name, out var tuple);
            var childElementType = tuple.InterfaceName;

            writer.WriteLine();
            writer.WriteLine($"    public ChildElementCollection<{childElementType}> Children {{ get; }}");

            writer.WriteLine();
            writer.WriteLine($"    IEnumerable<INode> INode.Children => Children;");

            writer.WriteLine();
            writer.WriteLine($"    INode? INode.Parent {{ get; set; }}");
            #endregion

            #region Properties

            foreach (var property in node.Properties)
            {
                var optionalSuffix = property.IsOptional ? "?" : "";

                writer.WriteLine();
                var @required = property.IsOptional ? "" : "required ";

                switch (property.Type)
                {
                    case PropertyType.Primitive p:
                        writer.WriteLine($"    public {@required}{CSharp(p)}{optionalSuffix} {property.Name} {{ get; init; }}");
                        break;
                    case PropertyType.Reference r:
                        var comment = $"// {(syntactial ? "the name of" : "a reference to")} a {r.Targets.Join(", ", " or ")} node";
                        writer.WriteLine($"    {comment}");

                        var setter = syntactial ? "init" : "private set";
                        @required = property.IsOptional || !syntactial ? "" : "required ";
                        var propertyType = syntactial ? "string" : r.Targets.Length == 1 ? r.Targets[0] : $"I{node.Name}{property.Name}";
                        writer.WriteLine($"    public {@required}{propertyType}{optionalSuffix} {property.Name} {{ get; {setter}; }}{(required == "required " ? "" : " = default!; // set via UpdateReferences")}");
                        break;
                    case PropertyType.Intrinsic b:
                        writer.WriteLine($"    public {@required}{b.Name}{optionalSuffix} {property.Name} {{ get; init; }}");
                        break;
                    default:
                        Trace.WriteLine($"Unexpected property type {property.Type} {property.Type.GetType().Name}");
                        // throw new ArgumentException(nameof(property.Type), $"Unexpected property type {property.Type} {property.Type.GetType().Name}");
                        break;
                }
            }
            #endregion

            if (syntactial)
            {
                var primitiveProperties = node.Properties
                    .Where(p => p.Type is PropertyType.Primitive or PropertyType.Intrinsic)
                    .Select(p => p.Name);

                writer.WriteLine();
                writer.WriteLine("    Sem.INode INode.Prepare()");
                writer.WriteLine("    {");
                writer.WriteLine($"        return new Sem.{node.Name}{{");
                foreach (var prop in primitiveProperties)
                {
                    writer.WriteLine($"            {prop} = {prop},");
                }
                writer.WriteLine($"        }};");
                writer.WriteLine("    }");
            }
            else
            {
                var referenceProperties = node.Properties
                     .Where(p => p.Type is PropertyType.Reference)
                     .Select(r => (r.Name, ((PropertyType.Reference)r.Type).Targets));

                writer.WriteLine();
                writer.WriteLine("    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)");
                writer.WriteLine("    {");
                foreach (var (prop, propTypes) in referenceProperties)
                {
                    var target = (propTypes.Length > 1) ? $"I{node.Name}{prop}" : propTypes[0];
                    writer.WriteLine($"        {prop} = node.Resolve<Syn.{node.Name}, {target}>(node => node.{prop}, tryGet)!;");
                }
                writer.WriteLine("    }");

                writer.WriteLine();
                writer.WriteLine($"    void INode.AddChild(Sem.INode node) => Children.Add(({childElementType})node);");
            }

            writer.WriteLine();
            writer.WriteLine($"    IEnumerable<(string, object?)> INode.Properties => [");

            // IEnumerable<(string, object?)> INode.Properties => [
            foreach (var prop in node.Properties)
            {
                switch (prop.Type)
                {
                    // case PropertyType.Reference:
                    //     writer.WriteLine($"        ( \"{prop.Name}\", {prop.Name} ),");
                    //     break;
                    default:
                        writer.WriteLine($"        ( \"{prop.Name}\", {prop.Name} ),");
                        break;
                }
            }
            writer.WriteLine($"    ]; ");

            writer.WriteLine($"}}");
        }
    }

    private static string CSharp(PropertyType type)
    {
        return type switch
        {
            PropertyType.Primitive p => p.Type switch
            {
                PropertyType.PrimitiveType.String => "string",
                PropertyType.PrimitiveType.Bool => "bool",
                PropertyType.PrimitiveType.Int => "int",
                _ => throw new InvalidEnumArgumentException(nameof(p.Type), (int)p.Type, typeof(PropertyType.PrimitiveType))
            },
            PropertyType.Reference r => r.Targets.Length == 1 ? r.Targets[0] : $"IOneOf<{string.Join(", ", r.Targets)}>",
            _ => type.ToString()
        };
    }


    static readonly string Header = """
        //------------------------------------------------------------------------------
        // <auto-generated>
        //     This code was generated by the OData model generator.
        //
        //     Changes to this file may cause incorrect behavior and will be lost if
        //     the code is regenerated.
        // </auto-generated>
        //------------------------------------------------------------------------------
        """;

}

