#nullable enable

#if !NETSTANDARD2_0
using System.Text;
#endif

namespace KSeF.Client.Validation;

/// <summary>
/// Zapewnia metody do wykrywania niedozwolonych znaków Unicode w treści faktury XML.
/// </summary>
public static class XmlUnicodeValidator
{
    /// <summary>
    /// Wyszukuje pierwszy niedozwolony znak Unicode w podanym tekście.
    /// </summary>
    /// <param name="value">Tekst do sprawdzenia.</param>
    /// <returns>Kod punktu w formacie "U+XXXX" pierwszego niedozwolonego znaku, albo <see langword="null"/>, jeśli nie znaleziono żadnego.</returns>
    public static string? FindDisallowedUnicodeCharacter(string value)
    {
#if NETSTANDARD2_0
        int index = 0;
        while (index < value.Length)
        {
            int codePoint = char.ConvertToUtf32(value, index);

            if (IsDisallowedUnicodeCharacter(codePoint))
            {
                return $"U+{codePoint:X4}";
            }

            index += char.IsSurrogatePair(value, index) ? 2 : 1;
        }
#else
        foreach (Rune rune in value.EnumerateRunes())
        {
            if (IsDisallowedUnicodeCharacter(rune.Value))
            {
                return $"U+{rune.Value:X4}";
            }
        }
#endif

        return null;
    }

    private static bool IsDisallowedUnicodeCharacter(int codePoint)
    {
        return
            (codePoint >= 0x007F && codePoint <= 0x0084) ||
            (codePoint >= 0x0086 && codePoint <= 0x009F) ||
            (codePoint >= 0xFDD0 && codePoint <= 0xFDEF) ||
            (codePoint >= 0x1FFFE && codePoint <= 0x10FFFF && (codePoint & 0xFFFE) == 0xFFFE);
    }
}
