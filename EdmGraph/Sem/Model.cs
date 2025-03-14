//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the OData model generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

namespace EdmGraph.Sem;

#region child  element interfaces

/// <summary>
/// interface for all children of Model.
/// implemented by <see cref="Schema"/>.
/// </summary>
public interface IModelChild : INode {}

/// <summary>
/// interface for all children of Schema.
/// implemented by <see cref="EnumType"/>, <see cref="ComplexType"/>, <see cref="PrimitiveType"/>, <see cref="Term"/>.
/// </summary>
public interface ISchemaChild : INode {}

/// <summary>
/// interface for all children of EnumType.
/// implemented by <see cref="Member"/>, <see cref="Annotation"/>.
/// </summary>
public interface IEnumTypeChild : INode {}

/// <summary>
/// interface for all children of Member.
/// implemented by <see cref="Annotation"/>.
/// </summary>
public interface IMemberChild : INode {}

/// <summary>
/// interface for all children of ComplexType.
/// implemented by <see cref="Property"/>, <see cref="Annotation"/>.
/// </summary>
public interface IComplexTypeChild : INode {}

/// <summary>
/// interface for all children of Property.
/// implemented by <see cref="Annotation"/>.
/// </summary>
public interface IPropertyChild : INode {}

/// <summary>
/// interface for all children of PrimitiveType.
/// implemented by <see cref="Annotation"/>.
/// </summary>
public interface IPrimitiveTypeChild : INode {}

/// <summary>
/// interface for all children of Annotation.
/// implemented by <see cref="Annotation"/>.
/// </summary>
public interface IAnnotationChild : INode {}

/// <summary>
/// interface for all children of Term.
/// implemented by <see cref="Annotation"/>.
/// </summary>
public interface ITermChild : INode {}
#endregion child  element interfaces

/// <summary>
/// Interface for model elements that can be referenced by Property.Type
/// Implemented by PrimitiveType, ComplexType, EnumType
/// </summary>
public interface IPropertyType : INode {
    String Name { get; }
}

/// <summary>
/// Interface for model elements that can be referenced by Term.Type
/// Implemented by PrimitiveType, ComplexType
/// </summary>
public interface ITermType : INode {
    String Name { get; }
}

public class Model : INode
{
    public Model() => Children = new(this);

    public ChildElementCollection<IModelChild> Children { get; }

    IEnumerable<INode> INode.Children => Children;

    INode? INode.Parent { get; set; }

    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)
    {
    }

    void INode.AddChild(Sem.INode node) => Children.Add((IModelChild)node);

    IEnumerable<(string, object?)> INode.Properties => [
    ]; 
}

public class Schema : INode, IModelChild
{
    public Schema() => Children = new(this);

    public ChildElementCollection<ISchemaChild> Children { get; }

    IEnumerable<INode> INode.Children => Children;

    INode? INode.Parent { get; set; }

    public required string Namespace { get; init; }

    public string? Alias { get; init; }

    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)
    {
    }

    void INode.AddChild(Sem.INode node) => Children.Add((ISchemaChild)node);

    IEnumerable<(string, object?)> INode.Properties => [
        ( "Namespace", Namespace ),
        ( "Alias", Alias ),
    ]; 
}

public class EnumType : INode, ISchemaChild, IPropertyType
{
    public EnumType() => Children = new(this);

    public ChildElementCollection<IEnumTypeChild> Children { get; }

    IEnumerable<INode> INode.Children => Children;

    INode? INode.Parent { get; set; }

    public required string Name { get; init; }

    public required bool IsFlags { get; init; }

    public UnderlyingType? UnderlyingType { get; init; }

    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)
    {
    }

    void INode.AddChild(Sem.INode node) => Children.Add((IEnumTypeChild)node);

    IEnumerable<(string, object?)> INode.Properties => [
        ( "Name", Name ),
        ( "IsFlags", IsFlags ),
        ( "UnderlyingType", UnderlyingType ),
    ]; 
}

public class Member : INode, IEnumTypeChild
{
    public Member() => Children = new(this);

    public ChildElementCollection<IMemberChild> Children { get; }

    IEnumerable<INode> INode.Children => Children;

    INode? INode.Parent { get; set; }

    public required string Name { get; init; }

    public int? Value { get; init; }

    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)
    {
    }

    void INode.AddChild(Sem.INode node) => Children.Add((IMemberChild)node);

    IEnumerable<(string, object?)> INode.Properties => [
        ( "Name", Name ),
        ( "Value", Value ),
    ]; 
}

public class ComplexType : INode, ISchemaChild, IPropertyType, ITermType
{
    public ComplexType() => Children = new(this);

    public ChildElementCollection<IComplexTypeChild> Children { get; }

    IEnumerable<INode> INode.Children => Children;

    INode? INode.Parent { get; set; }

    public required string Name { get; init; }

    public bool? Abstract { get; init; }

    // a reference to a ComplexType node
    public ComplexType? BaseType { get; private set; } = default!; // set via UpdateReferences

    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)
    {
        BaseType = node.Resolve<Syn.ComplexType, ComplexType>(node => node.BaseType, tryGet)!;
    }

    void INode.AddChild(Sem.INode node) => Children.Add((IComplexTypeChild)node);

    IEnumerable<(string, object?)> INode.Properties => [
        ( "Name", Name ),
        ( "Abstract", Abstract ),
        ( "BaseType", BaseType ),
    ]; 
}

public class Property : INode, IComplexTypeChild
{
    public Property() => Children = new(this);

    public ChildElementCollection<IPropertyChild> Children { get; }

    IEnumerable<INode> INode.Children => Children;

    INode? INode.Parent { get; set; }

    public required string Name { get; init; }

    // a reference to a PrimitiveType, ComplexType or EnumType node
    public IPropertyType Type { get; private set; } = default!; // set via UpdateReferences

    public bool? Nullable { get; init; }

    public string? DefaultValue { get; init; }

    public bool? IsCollection { get; init; }

    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)
    {
        Type = node.Resolve<Syn.Property, IPropertyType>(node => node.Type, tryGet)!;
    }

    void INode.AddChild(Sem.INode node) => Children.Add((IPropertyChild)node);

    IEnumerable<(string, object?)> INode.Properties => [
        ( "Name", Name ),
        ( "Type", Type ),
        ( "Nullable", Nullable ),
        ( "DefaultValue", DefaultValue ),
        ( "IsCollection", IsCollection ),
    ]; 
}

public class PrimitiveType : INode, ISchemaChild, IPropertyType, ITermType
{
    public PrimitiveType() => Children = new(this);

    public ChildElementCollection<IPrimitiveTypeChild> Children { get; }

    IEnumerable<INode> INode.Children => Children;

    INode? INode.Parent { get; set; }

    public required string Name { get; init; }

    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)
    {
    }

    void INode.AddChild(Sem.INode node) => Children.Add((IPrimitiveTypeChild)node);

    IEnumerable<(string, object?)> INode.Properties => [
        ( "Name", Name ),
    ]; 
}

public class Annotation : INode, IEnumTypeChild, IMemberChild, IComplexTypeChild, IPropertyChild, IPrimitiveTypeChild, IAnnotationChild, ITermChild
{
    public Annotation() => Children = new(this);

    public ChildElementCollection<IAnnotationChild> Children { get; }

    IEnumerable<INode> INode.Children => Children;

    INode? INode.Parent { get; set; }

    // a reference to a Term node
    public Term Term { get; private set; } = default!; // set via UpdateReferences

    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)
    {
        Term = node.Resolve<Syn.Annotation, Term>(node => node.Term, tryGet)!;
    }

    void INode.AddChild(Sem.INode node) => Children.Add((IAnnotationChild)node);

    IEnumerable<(string, object?)> INode.Properties => [
        ( "Term", Term ),
    ]; 
}

public class Term : INode, ISchemaChild
{
    public Term() => Children = new(this);

    public ChildElementCollection<ITermChild> Children { get; }

    IEnumerable<INode> INode.Children => Children;

    INode? INode.Parent { get; set; }

    public required string Name { get; init; }

    // a reference to a PrimitiveType or ComplexType node
    public ITermType Type { get; private set; } = default!; // set via UpdateReferences

    public string? Qualifier { get; init; }

    public required AppliesTo AppliesTo { get; init; }

    void INode.UpdateReferences(Syn.INode node, TryGet tryGet)
    {
        Type = node.Resolve<Syn.Term, ITermType>(node => node.Type, tryGet)!;
    }

    void INode.AddChild(Sem.INode node) => Children.Add((ITermChild)node);

    IEnumerable<(string, object?)> INode.Properties => [
        ( "Name", Name ),
        ( "Type", Type ),
        ( "Qualifier", Qualifier ),
        ( "AppliesTo", AppliesTo ),
    ]; 
}
