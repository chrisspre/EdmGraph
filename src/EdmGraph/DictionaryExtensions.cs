namespace EdmGraph;

public static class DictionaryExtensions
{
    // This extension method allows appending values to a list in a dictionary, creating the list if it doesn't exist.
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