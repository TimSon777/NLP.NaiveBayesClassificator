// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtensions
{
    // ReSharper disable once ReturnTypeCanBeEnumerable.Global
    public static string[] Split(this string src, IEnumerable<char> separators, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(separators);

        return src.Split(separators.Select(x => x.ToString()).ToArray(), options);
    }

    public static string ReplaceInvalid(this string src, IReadOnlySet<char> validChars, char replacement)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(validChars);

        return string.Join(string.Empty, InternalReplace(src, validChars, replacement));
    } 
    
    private static IEnumerable<char> InternalReplace(string src, IReadOnlySet<char> validChars, char replacement)
    {
        foreach (var e in src)
        {
            if (validChars.Contains(e))
            {
                yield return e;
            }
            else
            {
                yield return replacement;
            }
        }
    }
}