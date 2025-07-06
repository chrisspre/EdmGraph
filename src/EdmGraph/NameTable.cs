namespace EdmGraph;

using System;
using EdmGraph.Edm;
using Serilog;

// internal class NameTable
// {
//     internal readonly Dictionary<string, EdmElement> _elements = [];

//     public void Add(EdmElement element)
//     {
//         var targetPath = element.TargetPath;
//         if (targetPath is not null)
//         {
//             if (!_elements.TryAdd(targetPath, element))
//             {
//                 Log.Warning("Element with path {Path} already exists in NameTable", targetPath);
//             }
//         }
//     }

//     public void Add(string path, EdmElement element)
//     {
//         if (!_elements.TryAdd(path, element))
//         {
//             Log.Warning("Element with path {Path} already exists in NameTable", path);
//         }
//     }

//     internal T Get<T>(string targetPath) where T : EdmElement
//     {
//         if (_elements.TryGetValue(targetPath, out var element) && element is T typedElement)
//         {
//             return typedElement;
//         }
//         Log.Warning("Element with path '{targetPath}' not found in NameTable.", targetPath);
//         return default!; 
//     }

//     [Obsolete("Use Get<T>(string targetPath) instead.")]
//     public EdmElement this[string path]
//     {
//         get
//         {
//             if (_elements.TryGetValue(path, out var element))
//             {
//                 return element;
//             }
//             throw new KeyNotFoundException($"Element with path '{path}' not found in NameTable.");
//         }
//     }
// }
