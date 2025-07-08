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
                        if (stack.Count == 0)
                        {
                            throw new InvalidOperationException($"Missing parent for start element: {startElement.Name}");
                        }
                        var parent = stack.Peek();

                        IEdmElement element;
                        switch (startElement.Name)
                        {
                            case "Schema":
                                element = Schema.Create(startElement, parent, actions);
                                break;
                            case "EntityType":
                                element = EntityType.Create(startElement, parent, actions);
                                break;
                            case "ComplexType":
                                element = ComplexType.Create(startElement, parent, actions);
                                break;
                            case "Property":
                                element = Property.Create(startElement, parent, actions);
                                break;
                            case "NavigationProperty":
                                element = NavigationProperty.Create(startElement, parent, actions);
                                break;
                            case "Key":
                                element = Key.Create(startElement, parent, actions);
                                break;
                            case "PropertyRef":
                                element = PropertyRef.Create(startElement, parent, actions);
                                break;
                            case "EnumType":
                                element = EnumType.Create(startElement, parent, actions);
                                break;
                            case "Member":
                                element = Member.Create(startElement, parent, actions);
                                break;
                            case "TypeDefinition":
                                element = TypeDefinition.Create(startElement, parent, actions);
                                break;
                            case "Function":
                                element = Function.Create(startElement, parent, actions);
                                break;
                            case "Action":
                                element = Action.Create(startElement, parent, actions);
                                break;
                            case "Parameter":
                                element = Parameter.Create(startElement, parent, actions);
                                break;
                            case "ReturnType":
                                element = ReturnType.Create(startElement, parent, actions);
                                break;
                            case "EntityContainer":
                                element = EntityContainer.Create(startElement, parent, actions);
                                break;
                            case "EntitySet":
                                element = EntitySet.Create(startElement, parent, actions);
                                break;
                            case "Singleton":
                                element = Singleton.Create(startElement, parent, actions);
                                break;
                            case "FunctionImport":
                                element = FunctionImport.Create(startElement, parent, actions);
                                break;
                            case "ActionImport":
                                element = ActionImport.Create(startElement, parent, actions);
                                break;
                            case "NavigationPropertyBinding":
                                element = NavigationPropertyBinding.Create(startElement, parent, actions);
                                break;
                            case "ReferentialConstraint":
                                element = ReferentialConstraint.Create(startElement, parent, actions);
                                break;
                            case "OnDelete":
                                element = OnDelete.Create(startElement, parent, actions);
                                break;
                            case "Term":
                                element = Term.Create(startElement, parent, actions);
                                break;
                            case "Annotations":
                                element = Annotations.Create(startElement, parent, actions);
                                break;
                            case "Annotation":
                                element = Annotation.Create(startElement, parent, actions);
                                break;
                            case "Reference":
                                element = Reference.Create(startElement, parent, actions);
                                break;
                            case "Include":
                                element = Include.Create(startElement, parent, actions);
                                break;
                            case "IncludeAnnotations":
                                element = IncludeAnnotations.Create(startElement, parent, actions);
                                break;
                            default:
                                Log.Warning("Unhandled start element: {ElementName}", startElement.Name);
                                continue; // Skip unhandled elements, but continue processing
                        }

                        // Set LineInfo and push  the element to stack
                        element.LineInfo = startElement.LineInfo;
                        stack.Push(element);
                        
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
