namespace GLinter;

using System.Text.RegularExpressions;
using Humanizer;

public static class StringExtensions
{
    public static string ReplacePreservingCase(this string input, string pattern, string replacement)
    {

        string result = Regex.Replace(input, pattern, match =>
         {
             string original = match.Value;

             if (IsAllUpper(original))
                 return replacement.Transform(To.UpperCase);
             else if (IsAllLower(original))
                 return replacement.Transform(To.LowerCase);
             else if (IsPascalCase(original))
                 return replacement.Transform(To.TitleCase);
             else if (IsCamelCase(original))
                 return ToCamelCase(replacement);
             else
                 return replacement; // fallback
         }, RegexOptions.IgnoreCase);


        static bool IsCamelCase(string s) =>
            !string.IsNullOrEmpty(s) &&
            char.IsLower(s[0]) &&
            s.Skip(1).Any(char.IsUpper);

        static bool IsPascalCase(string s) =>
            !string.IsNullOrEmpty(s) &&
            char.IsUpper(s[0]) &&
            s.Skip(1).Any(char.IsLower);

        static bool IsAllUpper(string s) => s == s.ToUpper();
        static bool IsAllLower(string s) => s == s.ToLower();


        static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return char.ToLowerInvariant(s[0]) + s[1..];
        }

        return result;
    }

}