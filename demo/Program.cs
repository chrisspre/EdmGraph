using EdmGraph;
using EdmGraph.Syn;


var model = new Model
{
    Children = {
        new Schema
        {
            Namespace = "namespace",
            Alias = "alias",
            Children = {
                new EnumType
                {
                    Name = "enumTypeName",
                    IsFlags = true,
                    UnderlyingType = EdmGraph.UnderlyingType.Edm_Int32,
                    Children = {
                        new Member
                        {
                            Name = "member1",
                            Value = 1
                        },new Member
                        {
                            Name = "member2",
                        }
                    }
                },
                new ComplexType
                {
                    Name = "complexTypeName",
                    Abstract = true,
                    BaseType = "baseType",
                    Children = {
                        new Property
                        {
                            Name = "propertyName",
                            Type = "Edm.String",
                            IsCollection = false,
                            Nullable = false,
                            DefaultValue = "<unknown>"
                        }
                    }
                },
                new Term
                {
                    Name = "termName",
                    Type = "Edm.String",
                    Qualifier = "qualifier",
                    AppliesTo = EdmGraph.AppliesTo.ComplexType,
                }
            }
        }
    }
};

Console.WriteLine("// syntactic model");
model.Show();


var semanticModel = model.BuildSemanticModel();

Console.WriteLine("// syntactic model");
semanticModel.Show();

static class Ex
{
    public static void Show(this INode node, string indent = "")
    {
        Console.WriteLine($"{indent}{node.GetType().Name} {string.Join(", ", from p in node.Properties where p.Item2 is not null select $"{p.Item1}={p.Item2}")}");
        foreach (var child in node.Children)
        {
            Show(child, indent + "    ");
        }
    }


    public static void Show(this EdmGraph.Sem.INode node, string indent = "")
    {
        Console.WriteLine($"{indent}{node.GetType().Name} {string.Join(", ", from p in node.Properties where p.Item2 is not null select $"{p.Item1}={p.Item2}")}");
        foreach (var child in node.Children)
        {
            Show(child, indent + "    ");
        }
    }
}

