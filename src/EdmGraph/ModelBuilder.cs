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
        var stack = new Stack<EdmElement>();
        stack.Push(dataServices);

        foreach (var token in tokens)
        {
            switch (token)
            {
                case CsdlToken.StartElement startElement:
                    {
                        switch (startElement.Name)
                        {
                            case "Schema":
                                Schema.Create(startElement, stack, actions);
                                break;
                            case "EntityType":
                                EntityType.Create(startElement, stack, actions);
                                break;
                            case "ComplexType":
                                ComplexType.Create(startElement, stack, actions);
                                break;
                            case "Property":
                                Property.Create(startElement, stack, actions);
                                break;
                            case "NavigationProperty":
                                NavigationProperty.Create(startElement, stack, actions);
                                break;
                            case "Key":
                                Key.Create(startElement, stack, actions);
                                break;
                            case "PropertyRef":
                                PropertyRef.Create(startElement, stack, actions);
                                break;
                            case "EnumType":
                                EnumType.Create(startElement, stack, actions);
                                break;
                            case "Member":
                                Member.Create(startElement, stack, actions);
                                break;
                            case "TypeDefinition":
                                TypeDefinition.Create(startElement, stack, actions);
                                break;
                            case "Function":
                                Function.Create(startElement, stack, actions);
                                break;
                            case "Action":
                                Action.Create(startElement, stack, actions);
                                break;
                            case "Parameter":
                                Parameter.Create(startElement, stack, actions);
                                break;
                            case "ReturnType":
                                ReturnType.Create(startElement, stack, actions);
                                break;
                            case "EntityContainer":
                                EntityContainer.Create( startElement, stack, actions);
                                break;
                            case "EntitySet":
                                EntitySet.Create( startElement, stack, actions);
                                break;
                            case "Singleton":
                                Singleton.Create( startElement, stack, actions);
                                break;
                            case "FunctionImport":
                                FunctionImport.Create( startElement, stack, actions);
                                break;
                            case "ActionImport":
                                ActionImport.Create( startElement, stack, actions);
                                break;
                            case "NavigationPropertyBinding":
                                NavigationPropertyBinding.Create( startElement, stack, actions);
                                break;
                            case "ReferentialConstraint":
                                ReferentialConstraint.Create( startElement, stack, actions);
                                break;
                            case "OnDelete":
                                OnDelete.Create( startElement, stack, actions);
                                break;
                            case "Term":
                                Term.Create( startElement, stack, actions);
                                break;
                            case "Annotations":
                                Annotations.Create( startElement, stack, actions);
                                break;
                            case "Annotation":
                                Annotation.Create(startElement, stack, actions);
                                break;
                            case "Reference":
                                Reference.Create(startElement, stack, actions);
                                break;
                            case "Include":
                                Include.Create(startElement, stack, actions);
                                break;
                            case "IncludeAnnotations":
                                IncludeAnnotations.Create(startElement, stack, actions);
                                break;
                            default:
                                // For unhandled elements, we still need to track them for proper nesting
                                // but we don't create actual objects for them
                                Log.Warning("Unhandled start element: {ElementName}", startElement.Name);
                                break;
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
