namespace EdmGraph;

public enum UnderlyingType
{
    Edm_Byte, Edm_SByte, Edm_Int16, Edm_Int32, Edm_Int64
}

public enum AppliesTo
{
    EntitySet, EntityType, ComplexType, Property, NavigationProperty, Action, Function, Parameter, ReturnType, TypeDefinition, Term, Annotation, ValueAnnotation, Unknown
}