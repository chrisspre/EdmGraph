namespace EdmGraph;

/// <summary>
/// Provides extension methods for working with dictionaries
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Tries to append a value to a list in a dictionary, creating the list if it does not exist
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys</typeparam>
    /// <typeparam name="TValue">The type of the values in the lists</typeparam>
    /// <param name="dictionary">The dictionary to modify</param>
    /// <param name="key">The key for the list</param>
    /// <param name="value">The value to append to the list</param>
    /// <returns></returns>
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