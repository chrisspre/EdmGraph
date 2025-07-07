namespace EdmGraph;

using System.Net.Mime;
using EdmGraph.Edm;
using Microsoft.Win32;
using Serilog;

public static class ModelBuilder
{

    public static DataServices FromFile(string filePath)
    {
        var dataServices = new DataServices();
        var actions = new DeferredActionRegistry();

        var tokens = CsdlXmlReader.ReadTokens(filePath);

        Phase1(tokens, dataServices, actions);

        Phase2(actions, dataServices);

        return dataServices;
    }

    private static void Phase1(IEnumerable<CsdlToken> tokens, DataServices dataServices, DeferredActionRegistry actions)
    {
        var stack = new Stack<IEdmElement>();
        stack.Push(dataServices);

        foreach (var token in tokens)
        {
            switch (token)
            {
                case CsdlToken.StartElement startElement:
                    {
                        IEdmElement? element = null;
                        
                        switch (startElement.Name)
                        {
                            case "Schema":
                                element = Schema.Create(startElement, stack, actions);
                                break;
                            case "EntityType":
                                element = EntityType.Create(startElement, stack, actions);
                                break;
                            case "ComplexType":
                                element = ComplexType.Create(startElement, stack, actions);
                                break;
                            case "Property":
                                element = Property.Create(startElement, stack, actions);
                                break;
                            case "NavigationProperty":
                                element = NavigationProperty.Create(startElement, stack, actions);
                                break;
                            case "Key":
                                element = Key.Create(startElement, stack, actions);
                                break;
                            case "PropertyRef":
                                element = PropertyRef.Create(startElement, stack, actions);
                                break;
                            case "EnumType":
                                element = EnumType.Create(startElement, stack, actions);
                                break;
                            case "Member":
                                element = Member.Create(startElement, stack, actions);
                                break;
                            case "TypeDefinition":
                                element = TypeDefinition.Create(startElement, stack, actions);
                                break;
                            case "Function":
                                element = Function.Create(startElement, stack, actions);
                                break;
                            case "Action":
                                element = Action.Create(startElement, stack, actions);
                                break;
                            case "Parameter":
                                element = Parameter.Create(startElement, stack, actions);
                                break;
                            case "ReturnType":
                                element = ReturnType.Create(startElement, stack, actions);
                                break;
                            case "EntityContainer":
                                element = EntityContainer.Create(startElement, stack, actions);
                                break;
                            case "EntitySet":
                                element = EntitySet.Create(startElement, stack, actions);
                                break;
                            case "Singleton":
                                element = Singleton.Create(startElement, stack, actions);
                                break;
                            case "FunctionImport":
                                element = FunctionImport.Create(startElement, stack, actions);
                                break;
                            case "ActionImport":
                                element = ActionImport.Create(startElement, stack, actions);
                                break;
                            case "NavigationPropertyBinding":
                                element = NavigationPropertyBinding.Create(startElement, stack, actions);
                                break;
                            case "ReferentialConstraint":
                                element = ReferentialConstraint.Create(startElement, stack, actions);
                                break;
                            case "OnDelete":
                                element = OnDelete.Create(startElement, stack, actions);
                                break;
                            case "Term":
                                element = Term.Create(startElement, stack, actions);
                                break;
                            case "Annotations":
                                element = Annotations.Create(startElement, stack, actions);
                                break;
                            case "Annotation":
                                element = Annotation.Create(startElement, stack, actions);
                                break;
                            case "Reference":
                                element = Reference.Create(startElement, stack, actions);
                                break;
                            case "Include":
                                element = Include.Create(startElement, stack, actions);
                                break;
                            case "IncludeAnnotations":
                                element = IncludeAnnotations.Create(startElement, stack, actions);
                                break;
                            default:
                                // For unhandled elements, we still need to track them for proper nesting
                                // but we don't create actual objects for them
                                Log.Warning("Unhandled start element: {ElementName}", startElement.Name);
                                break;
                        }

                        // Set LineInfo and push to stack if an element was created
                        if (element != null)
                        {
                            element.LineInfo = startElement.LineInfo;
                            stack.Push(element);
                        }

                        break;
                    }
                case CsdlToken.EndElement endElement:
                    {
                        if (stack.Count == 0)
                        {
                            throw new InvalidOperationException("Unmatched end element: " + endElement.Name);
                        }

                        var popped = stack.Pop();

                        if (popped.GetType().Name != endElement.Name)
                        {
                            Console.WriteLine($"Warning: Mismatched end element. Expected {popped.GetType().Name}, but got {endElement.Name}");
                        }

                        break;
                    }

                case CsdlToken.AnnotationExpression annotationExpression:
                    break;
            }
        }
    }

    private static void Phase2(DeferredActionRegistry actions, DataServices model)
    {
        actions.ApplyActions(model);
    }
}
