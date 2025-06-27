namespace LabeledPropertyGraph.Net;

/// <summary>
/// Provides extension methods for working with dictionaries
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Tries to append a value to a list in a dictionary, creating the list if it does not exist.
    /// </summary>
    public static bool TryAppendValue<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out var list))
        {
            list = [];
            dictionary[key] = list;
        }
        list.Add(value);
        return true;
    }
}