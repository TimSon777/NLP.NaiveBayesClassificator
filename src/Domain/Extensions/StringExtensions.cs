using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtensions
{
    public static string RemoveHtmlTags(this string text, string replacement)
    {
        return Regex.Replace(text, "<.*?>", replacement);
    }

    public static string ReplaceInvalid(this string src, IReadOnlySet<char> validChars, string replacement)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(validChars);

        return string.Join(string.Empty, InternalReplace(src, validChars, replacement));
    } 
    
    private static IEnumerable<string> InternalReplace(string src, IReadOnlySet<char> validChars, string replacement)
    {
        foreach (var e in src)
        {
            if (validChars.Contains(e))
            {
                yield return e.ToString();
            }
            else
            {
                yield return replacement;
            }
        }
    }
}