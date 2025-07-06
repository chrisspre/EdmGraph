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
                                CreateSchema(stack, startElement);
                                break;
                            case "EntityType":
                                CreateEntityType(stack, actions, startElement);
                                break;
                            case "ComplexType":
                                CreateComplexType(stack, actions, startElement);
                                break;
                            case "Property":
                                CreateProperty(stack, actions, startElement);
                                break;
                            case "NavigationProperty":
                                CreateNavigationProperty(stack, actions, startElement);
                                break;
                            case "Key":
                                CreateKey(stack, startElement);
                                break;
                            case "PropertyRef":
                                CreatePropertyRef(stack, startElement);
                                break;
                            case "EnumType":
                                CreateEnumType(stack, startElement);
                                break;
                            case "Member":
                                CreateMember(stack, startElement);
                                break;
                            case "TypeDefinition":
                                CreateTypeDefinition(stack, startElement);
                                break;
                            case "Function":
                                CreateFunction(stack, startElement);
                                break;
                            case "Action":
                                CreateAction(stack, startElement);
                                break;
                            case "Parameter":
                                CreateParameter(stack, actions, startElement);
                                break;
                            case "ReturnType":
                                CreateReturnType(stack, startElement);
                                break;
                            case "EntityContainer":
                                CreateEntityContainer(stack, startElement);
                                break;
                            case "EntitySet":
                                CreateEntitySet(stack, startElement);
                                break;
                            case "Singleton":
                                CreateSingleton(stack, startElement);
                                break;
                            case "FunctionImport":
                                CreateFunctionImport(stack, startElement);
                                break;
                            case "ActionImport":
                                CreateActionImport(stack, startElement);
                                break;
                            case "NavigationPropertyBinding":
                                CreateNavigationPropertyBinding(stack, startElement);
                                break;
                            case "ReferentialConstraint":
                                CreateReferentialConstraint(stack, startElement);
                                break;
                            case "OnDelete":
                                CreateOnDelete(stack, startElement);
                                break;
                            case "Term":
                                CreateTerm(stack, startElement);
                                break;
                            case "Annotations":
                                CreateAnnotations(stack, startElement);
                                break;
                            case "Annotation":
                                CreateAndAddAnnotation(stack, actions, startElement);
                                break;
                            case "Reference":
                                CreateReference(stack, startElement);
                                break;
                            case "Include":
                                CreateInclude(stack, startElement);
                                break;
                            case "IncludeAnnotations":
                                CreateIncludeAnnotations(stack, startElement);
                                break;
                            default:
                                // For unhandled elements, we still need to track them for proper nesting
                                // but we don't create actual objects for them
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

    #region static factory methods for creating elements

    private static void CreateAndAddAnnotation(Stack<EdmElement> stack, DeferredActionRegistry actions, CsdlToken.StartElement startElement)
    {
        var annotation = new Annotation(
                startElement.Get("Qualifier", null))
        {
            LineInfo = startElement.LineInfo
        };

        // Handle the most common parent types that support annotations
        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case EntityType et:
                    et.AddAnnotation(annotation);
                    break;
                case ComplexType ct:
                    ct.AddAnnotation(annotation);
                    break;
                case Property prop:
                    prop.AddAnnotation(annotation);
                    break;
                case NavigationProperty nprop:
                    nprop.AddAnnotation(annotation);
                    break;
                case EntitySet es:
                    es.AddAnnotation(annotation);
                    break;
                case Singleton singleton:
                    singleton.AddAnnotation(annotation);
                    break;
                case Function func:
                    func.AddAnnotation(annotation);
                    break;
                case Action action:
                    action.AddAnnotation(annotation);
                    break;
                case Parameter param:
                    param.AddAnnotation(annotation);
                    break;
                case EnumType enumType:
                    enumType.AddAnnotation(annotation);
                    break;
                case Member member:
                    member.AddAnnotation(annotation);
                    break;
                case EntityContainer container:
                    container.AddAnnotation(annotation);
                    break;
                case Annotations annotations:
                    annotations.AddAnnotation(annotation);
                    break;
                case Reference reference:
                    reference.AddAnnotation(annotation);
                    break;
                case DataServices dataServices:
                    dataServices.AddAnnotation(annotation);
                    break;
                // Add more cases as needed
                default:
                    // If the parent type is not recognized
                    Log.Warning($"Unhandled parent type {parent} for annotation {annotation}", parent, annotation);
                    break;
            }
        }

        // deferred action for Term property resolution
        actions.Add(annotation, startElement.Get("Term"), static (element, path, resolver) =>
        {
            element.Term = resolver.Resolve<Term>(path);
        });

        stack.Push(annotation);
    }


    // Phase1 case handler methods
    private static void CreateSchema(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var schema = new Schema(
            startElement.Get("Namespace"),
            startElement.Get("Alias", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is DataServices dataServices)
        {
            dataServices.AddSchema(schema);
        }

        if (schema.Alias is not null)
        {
        }
        stack.Push(schema);
    }

    private static void CreateEntityType(Stack<EdmElement> stack, DeferredActionRegistry actions, CsdlToken.StartElement startElement)
    {
        var entityType = new EntityType(
            startElement.Get("Name"),
            startElement.Parse<bool>("Abstract"),
            startElement.Parse<bool>("OpenType"),
            startElement.Parse<bool>("HasStream"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddEntityType(entityType);
        }

        stack.Push(entityType);

        var baseType = startElement.Get("BaseType");
        if (baseType is not null && !string.IsNullOrEmpty(baseType))
        {
            actions.Add(entityType, startElement.Get("BaseType"), static (element, path, resolver) =>
            {
                element.BaseType = resolver.Resolve<EntityType>(path);
            });
        }
    }

    private static void CreateComplexType(Stack<EdmElement> stack, DeferredActionRegistry actions, CsdlToken.StartElement startElement)
    {
        var complexType = new ComplexType(
            startElement.Get("Name"),
            startElement.Parse<bool>("Abstract"),
            startElement.Parse<bool>("OpenType"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddComplexType(complexType);
        }

        // Add deferred action for BaseType property resolution if BaseType is not null
        var baseType = startElement.Get("BaseType", null);
        if (baseType is not null)
        {
            actions.Add(complexType, baseType, static (element, path, resolver) =>
            {
                element.BaseType = resolver.Resolve<ComplexType>(path);
            });
        }

        stack.Push(complexType);
    }

    private static void CreateProperty(Stack<EdmElement> stack, DeferredActionRegistry actions, CsdlToken.StartElement startElement)
    {
        var property = new Property(
            startElement.Get("Name"),
            startElement.ParseNullable<bool>("Nullable"),
            startElement.ParseNullable<int>("MaxLength"),
            startElement.Parse<bool>("Unicode", true),
            startElement.ParseNullable<int>("Precision"),
            startElement.Parse<Scale>("Scale"),
            startElement.ParseNullable<int>("SRID"),
            startElement.Get("DefaultValue", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case EntityType entityType:
                    entityType.AddProperty(property);
                    break;
                case ComplexType complexType:
                    complexType.AddProperty(property);
                    break;
            }
        }
        actions.Add(property, startElement.Get("Type"), SetType);
        stack.Push(property);

        // static helper method for the deferred action to set the Type property of Property, NavigationProperty, Parameter, etc.
        static void SetType(Property element, string path, IElementResolver resolver)
        {
            if (path.StartsWith("Collection(") && path.EndsWith(')'))
            {
                path = path[11..^1]; // Remove "Collection(" and ")"
                element.IsCollection = true;
            }
            else
            {
            }

            element.Type = resolver.Resolve<IPropertyType>(path);
            // Log.Information("Resolved property type {path} for property {element} to {result}", path, element, element.Type);
        }
    }


    private static void CreateNavigationProperty(Stack<EdmElement> stack, DeferredActionRegistry actions, CsdlToken.StartElement startElement)
    {
        var navigationProperty = new NavigationProperty(
            startElement.Get("Name"),
            startElement.ParseNullable<bool>("Nullable"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case EntityType entityType:
                    entityType.AddNavigationProperty(navigationProperty);
                    break;
                case ComplexType complexType:
                    complexType.AddNavigationProperty(navigationProperty);
                    break;
            }
        }

        // Add deferred action for Type property resolution
        actions.Add(navigationProperty, startElement.Get("Type"), SetType);

        // Add deferred action for Partner property resolution if Partner is not null
        var partner = startElement.Get("Partner", null);
        if (partner is not null)
        {
            actions.Add(navigationProperty, partner, static (element, path, resolver) =>
            {
                if (element.Type is null)
                {
                    Log.Warning("NavigationProperty {element} has a Partner {path} but Type is not set yet.", element, path);
                    return;
                }
                element.Partner = resolver.Resolve<NavigationProperty>(element.Type, path);
                Log.Verbose("{element}'s Partner={path} resolved to {element.Partner}", element, path, element.Partner);
            });
        }

        stack.Push(navigationProperty);


        static void SetType(NavigationProperty element, string path, IElementResolver resolver)
        {
            if (path.StartsWith("Collection(") && path.EndsWith(')'))
            {
                path = path[11..^1]; // Remove "Collection(" and ")"
                element.IsCollection = true;
            }

            element.Type = resolver.Resolve<EntityType>(path);
            if (element.Type is null)
            {
                Log.Warning("Element '{path}' not found while trying setting the Type property of {element}", path, element);
            }
        }
    }



    private static void CreateKey(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var key = new Key()
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is EntityType entityType)
        {
            entityType.SetKey(key);
        }

        stack.Push(key);
    }

    private static void CreatePropertyRef(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var propertyRef = new PropertyRef(startElement.Get("Name"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Key key)
        {
            key.AddPropertyRef(propertyRef);
        }

        // TODO: Add deferred action for property reference resolution
        // The Name should resolve to an actual Property in the parent EntityType
        // actions.Add(propertyRef, startElement.Get("Name"), static (element, path, resolver) => { 
        //     element.Property = resolver.Resolve<Property>(path);
        // });

        stack.Push(propertyRef);
    }

    private static void CreateEnumType(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        // https://docs.oasis-open.org/odata/odata-csdl-xml/v4.02/csd01/odata-csdl-xml-v4.02-csd01.html#ElementedmEnumType.17
        var enumType = new EnumType(
                startElement.Get("Name"),
                startElement.Parse<UnderlyingIntegerType>("UnderlyingType"),
                startElement.Parse<bool>("IsFlags"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddEnumType(enumType);
        }

        stack.Push(enumType);
    }



    private static void CreateMember(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var member = new Member(
            startElement.Get("Name"),
            startElement.ParseNullable<uint>("Value"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is EnumType enumType)
        {
            enumType.AddMember(member);
        }

        stack.Push(member);
    }

    private static void CreateTypeDefinition(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var typeDefinition = new TypeDefinition(
            startElement.Get("Name"),
            startElement.Get("UnderlyingType"),
            startElement.ParseNullable<int>("MaxLength"),
            startElement.Parse<bool>("Unicode", true),
            startElement.ParseNullable<int>("Precision"),
            startElement.Get("Scale", null),
            startElement.ParseNullable<int>("SRID")
        )
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case Schema schema:
                    schema.AddTypeDefinition(typeDefinition);
                    break;
                default:
                    Log.Error("TypeDefinition {Name} can not be added to {parent}.", typeDefinition.Name, parent);
                    break;
            }
        }

        // TODO: Add deferred action for UnderlyingType property resolution
        // actions.Add(typeDefinition, startElement.Get("UnderlyingType"), static (element, path, resolver) => { 
        //     element.UnderlyingType = resolver.Resolve<IUnderlyingType>(path);
        // });

        stack.Push(typeDefinition);
    }

    private static void CreateFunction(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var function = new Function(
            startElement.Get("Name"),
            startElement.Parse<bool>("IsBound"),
            startElement.Get("EntitySetPath", null),
            startElement.Parse<bool>("IsComposable"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddFunction(function);
        }

        stack.Push(function);
    }

    private static void CreateAction(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var action = new Action(
            startElement.Get("Name"),
            startElement.Parse<bool>("IsBound"),
            startElement.Get("EntitySetPath", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddAction(action);
        }

        stack.Push(action);
    }

    private static void CreateParameter(Stack<EdmElement> stack, DeferredActionRegistry actions, CsdlToken.StartElement startElement)
    {
        var parameter = new Parameter(
            startElement.Get("Name"),
            startElement.Parse<bool>("Nullable", true))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case Function function:
                    function.AddParameter(parameter);
                    break;
                case Action action:
                    action.AddParameter(parameter);
                    break;
            }
        }

        // TODO: Add deferred action for Type property resolution
        // actions.Add(parameter, startElement.Get("Type"), SetType);
        actions.Add(parameter, startElement.Get("Type"), SetType);

        stack.Push(parameter);

        // static helper method for the deferred action to set the Type property of Property, NavigationProperty, Parameter, etc.
        static void SetType(Parameter parameter, string path, IElementResolver resolver)
        {
            if (path.StartsWith("Collection(") && path.EndsWith(')'))
            {
                path = path[11..^1]; // Remove "Collection(" and ")"
                // element.IsCollection = true;
            }
            parameter.Type = resolver.Resolve<IPropertyType>(path);
            if (parameter.Type is null)
            {
                Log.Warning("Element '{path}' not found while trying setting the Type property of {element}", path, parameter);
            }
        }
    }

    private static void CreateReturnType(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var returnType = new ReturnType(
            startElement.Get("Type"),
            startElement.Parse<bool>("Nullable", true))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Function function)
        {
            function.SetReturnType(returnType);
        }

        // TODO: Add deferred action for Type property resolution
        // actions.Add(returnType, startElement.Get("Type"), static (element, path, resolver) => { 
        //     element.Type = resolver.Resolve<IReturnType>(path);
        // });

        stack.Push(returnType);
    }

    private static void CreateEntityContainer(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var entityContainer = new EntityContainer(
            startElement.Get("Name"),
            startElement.Get("Extends", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddEntityContainer(entityContainer);
        }

        // TODO: Add deferred action for Extends property resolution if Extends is not null
        var extends = startElement.Get("Extends", null);
        if (extends is not null)
        {
            // actions.Add(entityContainer, extends, static (element, path, resolver) => { 
            //     element.Extends = resolver.Resolve<EntityContainer>(path);
            // });
        }

        stack.Push(entityContainer);
    }

    private static void CreateEntitySet(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var entitySet = new EntitySet(
            startElement.Get("Name"),
            startElement.Get("EntityType"),
            startElement.Parse<bool>("IncludeInServiceDocument", true))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is EntityContainer entityContainer)
        {
            entityContainer.AddEntitySet(entitySet);
        }

        // TODO: Add deferred action for EntityType property resolution
        // actions.Add(entitySet, startElement.Get("EntityType"), static (element, path, resolver) => { 
        //     element.EntityType = resolver.Resolve<EntityType>(path);
        // });

        stack.Push(entitySet);
    }

    private static void CreateSingleton(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var singleton = new Singleton(
            startElement.Get("Name"),
            startElement.Get("Type"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is EntityContainer entityContainer)
        {
            entityContainer.AddSingleton(singleton);
        }

        // TODO: Add deferred action for Type property resolution
        // actions.Add(singleton, startElement.Get("Type"), static (element, path, resolver) => { 
        //     element.Type = resolver.Resolve<EntityType>(path);
        // });

        stack.Push(singleton);
    }

    private static void CreateFunctionImport(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var functionImport = new FunctionImport(
            startElement.Get("Name"),
            startElement.Get("Function"),
            startElement.Get("EntitySet", null),
            startElement.Parse<bool>("IncludeInServiceDocument", false))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is EntityContainer entityContainer)
        {
            entityContainer.AddFunctionImport(functionImport);
        }

        // TODO: Add deferred action for Function property resolution
        // actions.Add(functionImport, startElement.Get("Function"), static (element, path, resolver) => { 
        //     element.Function = resolver.Resolve<Function>(path);
        // });

        // TODO: Add deferred action for EntitySet property resolution if EntitySet is not null
        var entitySet = startElement.Get("EntitySet", null);
        if (entitySet is not null)
        {
            // actions.Add(functionImport, entitySet, static (element, path, resolver) => { 
            //     element.EntitySet = resolver.Resolve<EntitySet>(path);
            // });
        }

        stack.Push(functionImport);
    }

    private static void CreateActionImport(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var actionImport = new ActionImport(
            startElement.Get("Name"),
            startElement.Get("Action"),
            startElement.Get("EntitySet", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is EntityContainer entityContainer)
        {
            entityContainer.AddActionImport(actionImport);
        }

        // TODO: Add deferred action for Action property resolution
        // actions.Add(actionImport, startElement.Get("Action"), static (element, path, resolver) => { 
        //     element.Action = resolver.Resolve<Action>(path);
        // });

        // TODO: Add deferred action for EntitySet property resolution if EntitySet is not null
        var entitySet = startElement.Get("EntitySet", null);
        if (entitySet is not null)
        {
            // actions.Add(actionImport, entitySet, static (element, path, resolver) => { 
            //     element.EntitySet = resolver.Resolve<EntitySet>(path);
            // });
        }

        stack.Push(actionImport);
    }

    private static void CreateNavigationPropertyBinding(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var navigationPropertyBinding = new NavigationPropertyBinding(
            startElement.Get("Path"),
            startElement.Get("Target"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent))
        {
            switch (parent)
            {
                case EntitySet entitySet:
                    entitySet.AddNavigationPropertyBinding(navigationPropertyBinding);
                    break;
                case Singleton singleton:
                    singleton.AddNavigationPropertyBinding(navigationPropertyBinding);
                    break;
            }
        }

        // TODO: Add deferred action for Path property resolution (NavigationProperty)
        // actions.Add(navigationPropertyBinding, startElement.Get("Path"), static (element, path, resolver) => { 
        //     element.NavigationProperty = resolver.Resolve<NavigationProperty>(path);
        // });

        // TODO: Add deferred action for Target property resolution (EntitySet)
        // actions.Add(navigationPropertyBinding, startElement.Get("Target"), static (element, path, resolver) => { 
        //     element.Target = resolver.Resolve<EntitySet>(path);
        // });

        stack.Push(navigationPropertyBinding);
    }

    private static void CreateReferentialConstraint(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var referentialConstraint = new ReferentialConstraint()
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is NavigationProperty navigationProperty)
        {
            navigationProperty.AddReferentialConstraint(referentialConstraint);
        }

        stack.Push(referentialConstraint);
    }

    private static void CreateOnDelete(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var onDelete = new OnDelete(startElement.Get("Action"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is NavigationProperty navigationProperty)
        {
            navigationProperty.SetOnDelete(onDelete);
        }

        stack.Push(onDelete);
    }

    private static void CreateTerm(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var term = new Term(
            startElement.Get("Name"),
            startElement.Get("Type"),
            startElement.Get("BaseTerm", null),
            startElement.Get("AppliesTo", null),
            startElement.Get("DefaultValue", null),
            startElement.Parse<bool>("Nullable", true))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddTerm(term);
        }

        // TODO: Add deferred action for Type property resolution
        // actions.Add(term, startElement.Get("Type"), static (element, path, resolver) => { 
        //     element.Type = resolver.Resolve<ITermType>(path);
        // });

        // TODO: Add deferred action for BaseTerm property resolution if BaseTerm is not null
        var baseTerm = startElement.Get("BaseTerm", null);
        if (baseTerm is not null)
        {
            // actions.Add(term, baseTerm, static (element, path, resolver) => { 
            //     element.BaseTerm = resolver.Resolve<Term>(path);
            // });
        }

        stack.Push(term);
    }

    private static void CreateAnnotations(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var annotations = new Annotations(
            startElement.Get("Target"),
            startElement.Get("Qualifier", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Schema schema)
        {
            schema.AddAnnotations(annotations);
        }

        // TODO: Add deferred action for Target property resolution
        // The Target should resolve to the actual EdmElement being annotated
        // actions.Add(annotations, startElement.Get("Target"), static (element, path, resolver) => { 
        //     element.Target = resolver.Resolve<EdmElement>(path);
        // });

        stack.Push(annotations);
    }

    private static void CreateReference(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var reference = new Reference(startElement.Get("Uri"))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Edmx edmx)
        {
            edmx.AddReference(reference);
        }

        stack.Push(reference);
    }

    private static void CreateInclude(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var include = new Include(
            startElement.Get("Namespace"),
            startElement.Get("Alias", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Reference reference)
        {
            reference.AddInclude(include);
        }

        stack.Push(include);
    }

    private static void CreateIncludeAnnotations(Stack<EdmElement> stack, CsdlToken.StartElement startElement)
    {
        var includeAnnotations = new IncludeAnnotations(
            startElement.Get("TermNamespace"),
            startElement.Get("Qualifier", null),
            startElement.Get("TargetNamespace", null))
        {
            LineInfo = startElement.LineInfo
        };

        if (stack.TryPeek(out var parent) && parent is Reference reference)
        {
            reference.AddIncludeAnnotations(includeAnnotations);
        }

        stack.Push(includeAnnotations);
    }

    #endregion
    // End of Phase1 case handler methods

    private static void Phase2(DeferredActionRegistry actions, DataServices model)
    {
        actions.ApplyActions(model);
    }
}


// Properties requiring deferred resolution:
// EntityType.BaseType - References another EntityType
// ComplexType.BaseType - References another ComplexType
// Property.Type - References a type (already implemented in line 320)
// NavigationProperty.Type - References an EntityType
// NavigationProperty.Partner - References another NavigationProperty
// PropertyRef.Name - References a Property in the parent EntityType
// Parameter.Type - References a parameter type
// ReturnType.Type - References a return type
// EntityContainer.Extends - References another EntityContainer
// EntitySet.EntityType - References an EntityType
// Singleton.Type - References an EntityType
// FunctionImport.Function - References a Function
// FunctionImport.EntitySet - References an EntitySet (optional)
// ActionImport.Action - References an Action
// ActionImport.EntitySet - References an EntitySet (optional)
// NavigationPropertyBinding.Path - References a NavigationProperty
// NavigationPropertyBinding.Target - References an EntitySet
// Term.Type - References a term type
// Term.BaseTerm - References another Term (optional)
// TypeDefinition.UnderlyingType - References an underlying type
// Annotations.Target - References the EdmElement being annotated
// Annotation.Term - References a Term


// new Property("BaseTerm", PropertyType.Reference("Term"), IsOptional: true),
// new Property("BaseType", PropertyType.Reference("ComplexType"), IsOptional: true),
// new Property("BaseType", PropertyType.Reference("EntityType"), IsOptional: true),
// new Property("EntitySet", PropertyType.Reference("EntitySet")),
// new Property("EntityType", PropertyType.Reference("EntityType")),
// new Property("Extends", PropertyType.Reference("EntityContainer"), IsOptional: true),

// new Property("Action", PropertyType.Reference("$Action")),
// new Property("Function", PropertyType.Reference("Function")),

// new Property("Name", PropertyType.Reference(new PropertyPath("../.."), ["Property"])),
// new Property("Partner", PropertyType.Reference(["NavigationProperty"]), IsOptional: true),
// new Property("Partner", PropertyType.Reference(new PropertPath("./Type"), ["NavigationProperty"]), IsOptional: true),
// new Property("Property", PropertyType.Reference()),
// new Property("ReferencedProperty", PropertyType.Reference()),
// new Property("Target", PropertyType.Reference()),
// new Property("Term", PropertyType.Reference("Term")),
// new Property("Type", PropertyType.Reference("EntityType", "ComplexType", "PrimitiveType", "EnumType", "TypeDefinition")),
// new Property("Type", PropertyType.Reference("EntityType")),
// new Property("Type", PropertyType.Reference("EntityType")),
// new Property("Type", PropertyType.Reference("PrimitiveType", "ComplexType", "EntityType", "EnumType", "TypeDefinition")),
// new Property("Type", PropertyType.Reference("PrimitiveType", "ComplexType", "EntityType", "EnumType", "TypeDefinition")),
// new Property("Type", PropertyType.Reference("PrimitiveType", "ComplexType", "EnumType","TypeDefinition")),
// new Property("UnderlyingType", PropertyType.Reference("PrimitiveType"), IsOptional: true),
// new Property("UnderlyingType", PropertyType.Reference("PrimitiveType")),