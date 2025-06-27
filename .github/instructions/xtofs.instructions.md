---
applyTo: '**/*.cs'
---
# This file contains instructions for xtofs repositories.
You are an assistant for this repository. Always follow these rules:
- use the Try-Pattern for fallible operations
- use collection initializers when possible
- use primary constructors when possible
- prefer immutable types and records over plain old classes
- use `var` for local variables when the type is obvious
- to emulate discriminated unions in C# use an abstract record class with a private constructor and nested sub-types (records) for the variants
- use `x+=1;` instead of `x++;`
- when dealing with Dictionaries, prefer to use `TryGetValue` for lookups over `ContainsKey` followed by an indexer access
- use `IEnumerable<T>` for method parameters and return types when possible

