using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static class StringNormalizer
{
    private static readonly Dictionary<string, string> LigatureReplacements = new()
    {
        // French, German
        ["œ"] = "oe",
        ["Œ"] = "Oe",
        ["æ"] = "ae",
        ["Æ"] = "Ae",
        ["ß"] = "ss",
    };

    public static string NormalizeString(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        // Replace ligatures and special letters
        foreach (var pair in LigatureReplacements)
            input = input.Replace(pair.Key, pair.Value);

        // Normalize the string to FormD (decomposed form), which separates characters from their diacritical marks.
        string normalizedString = input.Normalize(NormalizationForm.FormD);

        // Create a StringBuilder to hold the normalized ASCII characters.
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            // Check if the character is a non-spacing mark (diacritical mark).
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                // Append the character to the StringBuilder if it's not a diacritical mark.
                stringBuilder.Append(c);
            }
        }

        // Convert the StringBuilder to a string and normalize it to FormC (composed form).
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
