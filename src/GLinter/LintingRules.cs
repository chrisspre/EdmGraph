namespace EdmGraph.Console;

using System;
using EdmGraph.Edm;

public static class LintingRules
{
    public static IEnumerable<string> FlagsEnumValuesMustBeAPowerOfTwo(EnumType enumType)
    {
        if (!enumType.IsFlags)
        {
            yield break;
        }
        var invalidValues =
              from member in enumType.Members
              let val = member.Value
              where val.HasValue && !(val.Value == 0 || System.Numerics.BitOperations.PopCount(val.Value) == 1)
              select (((IEdmElement)member).Name, val);

        if (invalidValues.Any())
        {
            yield return $"All values of a flags enum MUST be a power of 2. Invalid values: {string.Join(",", invalidValues.Select(m => $"{m.Name}:{m.val}"))}";
        }
    }

    public static IEnumerable<string> DoNotUseOpenTypes(EntityType element)
    {
        if (element.OpenType)
        {
            yield return "Do not use open entity types.";
        }
    }

    public static IEnumerable<string> DoNotUseOpenTypes(ComplexType element)
    {
        if (element.OpenType)
        {
            yield return "Do not use open complex types.";
        }
    }

    public static IEnumerable<string> CollectionsShouldNotBeNullable(Property element)
    {
        if (element.IsCollection && (element.Nullable ?? false))
        {
            yield return "Collections Should not be nullable.";
        }
    }
    public static IEnumerable<string> PropertyTypeShouldNotBeANullableEnum(Property property)
    {
        if ((property.Nullable ?? false) && property.Type is EnumType enumType)
        {
            yield return "Properties SHOULD not have nullable enum types. Add a 'none'/'null' member to the enum.";
        }
    }


    public static IEnumerable<string> PropertyNameShouldNotHaveATypeNamePrefix(Property property)
    {
        var parentTypeName = property.ParentElement?.Name!;
        if (property.Name.StartsWith(parentTypeName))
        {
            yield return $"Property name should not repeat parent's name\"{parentTypeName}\" as a prefix.";
        }
    }

    private static readonly HashSet<string> NamesToAvoidInsteadOfModifier = ["lastChangedBy", "lastUpdatedBy", "changedBy", "updatedBy", "lastChangedDateTime", "lastUpdatedDateTime"];

    public static IEnumerable<string> UseModifiedInsteadOfChangedOrUpdated(Property property)
    {
        var name = property.Name;
        if (NamesToAvoidInsteadOfModifier.Contains(name))
        {
            var suggestion = name.ReplacePreservingCase("(changed|updated)", "modified");
            var message = $"Prefer 'modified' over `changed` or `updated` in name of property `{name}`. Suggestion: `{suggestion}`";
            yield return message;
        }
    }


    private static readonly HashSet<string> NamesToAvoidInsteadOfCreated = ["creationDateTime", "createDateTime", "creationBy"];

    public static IEnumerable<string> UseCreatedInsteadOfCreateOrCreation(Property property)
    {
        var name = property.Name;
        if (NamesToAvoidInsteadOfCreated.Contains(name))
        {
            var suggestion = name.ReplacePreservingCase(@"(create|creation)", "created");
            var message = $"Prefer `created` over `create` or `creation` in name of property {name}. Suggestion: '{suggestion}'.";

            yield return message;
        }
    }
}
