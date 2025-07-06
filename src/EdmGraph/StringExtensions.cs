

namespace EdmGraph;

public static class StringExtensions
{
    public static bool IsSimpleIdentifier(this string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 126)
            return false;

        if (!char.IsLetter(value[0]) && value[0] != '_')
            return false;

        for (int i = 1; i < value.Length; i++)
        {
            if (!char.IsLetterOrDigit(value[i]) && value[i] != '_')
                return false;
        }

        return true;
    }
}