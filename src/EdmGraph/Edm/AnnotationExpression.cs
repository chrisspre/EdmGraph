namespace EdmGraph.Edm;

/// <summary>
/// Base class for annotation expressions
/// </summary>
public abstract record AnnotationExpression;


/// <summary>
/// Represents a collection expression in annotations
/// </summary>
public sealed record CollectionExpression(
    IReadOnlyList<AnnotationExpression> Items) : AnnotationExpression;

/// <summary>
/// Represents a record expression in annotations
/// </summary>
public sealed record RecordExpression(
    string? Type = null) : AnnotationExpression
{
    private readonly List<PropertyValue> _propertyValues = [];
    public IReadOnlyCollection<PropertyValue> PropertyValues => _propertyValues;

    internal void AddPropertyValue(PropertyValue propertyValue) => _propertyValues.Add(propertyValue);
}

/// <summary>
/// Represents a property value in a record expression
/// </summary>
public sealed record PropertyValue(
    string Property,
    AnnotationExpression Value);

/// <summary>
/// Represents an if-then-else expression
/// </summary>
public sealed record IfExpression(
    AnnotationExpression Condition,
    AnnotationExpression ThenExpression,
    AnnotationExpression? ElseExpression = null) : AnnotationExpression;
