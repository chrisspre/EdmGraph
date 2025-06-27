namespace EdmGraph;

/// <summary>
/// Utility class for string operations.
/// </summary>
public static class StringUtils
{
    /// <summary>
    /// Reverses a string.
    /// </summary>
    /// <param name="input">The string to reverse</param>
    /// <returns>The reversed string</returns>
    public static string Reverse(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        char[] chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    /// <summary>
    /// Counts the number of words in a string.
    /// </summary>
    /// <param name="input">The string to count words in</param>
    /// <returns>The number of words</returns>
    public static int CountWords(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        return input.Split(new char[] { ' ', '\t', '\n', '\r' },
                          StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Capitalizes the first letter of each word in a string.
    /// </summary>
    /// <param name="input">The string to capitalize</param>
    /// <returns>The capitalized string</returns>
    public static string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }
}
