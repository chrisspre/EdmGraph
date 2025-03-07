using EdmGraph.Syn;
using Sem = EdmGraph.Sem;


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
                            Name = "memberName",
                            Value = 0
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

model.Show();

var semanticModel = model.BuildSemanticModel();

semanticModel.Show();

static class Ex
{
    public static void Show(this INode node, string indent = "")
    {
        // Console.WriteLine($"{indent}{node.GetType().Name} {node.Attributes.Format()}");
        Console.WriteLine($"{indent}{node.GetType().Name} ");
        foreach (var child in node.Children)
        {
            Show(child, indent + "    ");
        }
    }


    public static void Show(this Sem.INode node, string indent = "")
    {
        // Console.WriteLine($"{indent}{node.GetType().Name} {node.Attributes.Format()}");
        Console.WriteLine($"{indent}{node.GetType().Name} ");
        foreach (var child in node.Children)
        {
            Show(child, indent + "    ");
        }
    }
}

