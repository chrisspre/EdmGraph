namespace omm.generator;
using omm.generator.Model;


/// <summary>
/// Meta Model of OData 4.01 CSDL 
/// </summary>
internal class OData401
{
    public static Dictionary<string, ModelElement> Model { get; } = new Dictionary<string, ModelElement>() {
            new ModelElement("Model", ["Schema"],
            Properties:[
            ]),

        new ModelElement("Schema", ["EnumType", "ComplexType", "PrimitiveType", "Term"],
            Properties:[
                new Property("Namespace", PropertyType.String),
                new Property("Alias", PropertyType.String, IsOptional: true),
            ]),

        new ModelElement("EnumType", ["Member", "Annotation"],
            Properties:[
                new Property("Name", PropertyType.String),
                new Property("IsFlags", PropertyType.Boolean),
                new Property("UnderlyingType", new PropertyType.Intrinsic("UnderlyingType"), IsOptional: true)
            ]),


        new ModelElement("Member", ["Annotation"],
            Properties:[
                new Property("Name", PropertyType.String),
                new Property("Value", PropertyType.Int, IsOptional: true)
            ]),

        // https://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html#sec_ComplexType
        new ModelElement("ComplexType", ["Property", "Annotation"],
            Properties:[
                new Property("Name", PropertyType.String),
                new Property("Abstract", PropertyType.Boolean , IsOptional: true, DefaultValue: "false"),
                new Property("BaseType", PropertyType.CreateReference("ComplexType"), IsOptional: true)
            ]
        ),

        // https://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html#sec_TypeFacets
        new ModelElement("Property", ["Annotation"], [
                new Property("Name", PropertyType.String),
                new Property("Type", PropertyType.CreateReference("PrimitiveType", "ComplexType", "EnumType")),
                new Property("Nullable", PropertyType.Boolean, IsOptional: true), // If no value is specified for a collection-valued property, the client cannot assume any default value
                new Property("DefaultValue", PropertyType.String, IsOptional: true),
                new Property("IsCollection", PropertyType.Boolean, IsOptional:true)
                // n.b. IsCollection is not an XML attribute in CSDL XML but a property in the meta model.
                // It is indicate in the string representation of the `Type` XML attribute (e.g. Type="Collection(Edm.String)")
            ]),

        new ModelElement("PrimitiveType", ["Annotation"],
            Properties: [new Property("Name", PropertyType.String)]),

        new ModelElement("Annotation", ["Annotation"],
            Properties: [new Property("Term", PropertyType.CreateReference("Term"))]),

        new ModelElement("Term", ["Annotation"], Properties: [
            new Property("Name", PropertyType.String),
            new Property("Type", new PropertyType.Reference(["PrimitiveType","ComplexType"])),
            new Property("Qualifier", PropertyType.String, IsOptional: true),
            new Property("AppliesTo", new PropertyType.Intrinsic("AppliesTo"))
        ]),
    };
}