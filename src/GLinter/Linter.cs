namespace EdmGraph.Console;

using EdmGraph.Edm;

/// <summary>
/// Represents a linting rule that can be applied to an EdmElement.
/// </summary>
public delegate IEnumerable<string> LintingRule<in T>(T element) where T : EdmElement;

public record LintingRuleInfo(string Id, Func<EdmElement, IEnumerable<string>> Rule);

public static class Linter
{
    // Define the rules with their metadata
    public static readonly List<LintingRuleInfo> Rules = [
        Rule<EnumType>("GL0001", LintingRules.FlagsEnumValuesMustBeAPowerOfTwo),
        Rule<EntityType>("GL0002", LintingRules.DoNotUseOpenTypes),
        Rule<ComplexType>("GL0003", LintingRules.DoNotUseOpenTypes),
        Rule<Property>("GL0004", LintingRules.CollectionsShouldNotBeNullable),
        Rule<Property>("GL0005", LintingRules.PropertyTypeShouldNotBeANullableEnum),
        Rule<Property>("GL0006", LintingRules.UseCreatedInsteadOfCreateOrCreation),
        Rule<Property>("GL0007", LintingRules.UseModifiedInsteadOfChangedOrUpdated),
    ];

    // Factory method for rule creation
    // This method creates a LintingRuleInfo for a specific type of EdmElement
    private static LintingRuleInfo Rule<T>(string id, LintingRule<T> rule) where T : EdmElement
    {
        return new(id, element => element is T typedElement ? rule(typedElement) : []);
    }
    
    // Method to run rules for all elements in a data services model
    public static IEnumerable<(string,string, EdmElement)> RunRules(DataServices dataServices)
    {
        // Traverse all elements in the model
        var allElements = TraverseElements(dataServices);

        foreach (var element in allElements)
        {
            foreach (var (id,message) in RunRulesSingleElement(element))
            {
                yield return (id, message, element);
            }
        }
    }

    // Method to run all applicable rules for a given element - no reflection needed
    private static IEnumerable<(string, string)> RunRulesSingleElement(EdmElement element)
    {
        foreach (var ruleInfo in Rules)
        {
            foreach (var message in ruleInfo.Rule(element))
            {
                yield return (ruleInfo.Id, message);
            }
        }
    }

    // Helper method to traverse all elements in the model
    private static IEnumerable<EdmElement> TraverseElements(EdmElement root)
    {
        var stack = new Stack<EdmElement>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            // Push children in reverse order to maintain traversal order
            foreach (var child in current.Children.Reverse())
            {
                stack.Push(child);
            }
        }
    }
}
