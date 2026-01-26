namespace KSeF.Client.Validation;

/// <summary>
/// Klasa pomocnicza do walidacji identyfikatorów (NIP, IDWew) na podstawie sum kontrolnych.
/// </summary>
public static class IdentifierValidators
{
    /// <summary>
    /// Waliduje format i sumę kontrolną NIP (Numer Identyfikacyjny Podatnika).
    /// </summary>
    /// <param name="nip">NIP do walidacji (10 cyfr).</param>
    /// <returns><c>true</c> jeśli NIP jest prawidłowy; w przeciwnym razie <c>false</c>.</returns>
    public static bool IsValidNip(string nip)
    {
        if (string.IsNullOrWhiteSpace(nip) || nip.Length != 10 || !nip.All(char.IsDigit))
        {
            return false;
        }

        int[] weights = { 6, 5, 7, 2, 3, 4, 5, 6, 7 };

        int checksum = 0;
        for (int i = 0; i < 9; i++)
        {
            checksum += (nip[i] - '0') * weights[i];
        }

        int calculatedCheckDigit = checksum % 11;

        if (calculatedCheckDigit == 10)
        {
            return false;
        }

        return calculatedCheckDigit == (nip[9] - '0');
    }

    /// <summary>
    /// Waliduje format i sumę kontrolną identyfikatora wewnętrznego (IDWew).
    /// </summary>
    /// <param name="value">Identyfikator wewnętrzny do walidacji.</param>
    /// <returns><c>true</c> jeśli identyfikator jest prawidłowy; w przeciwnym razie <c>false</c>.</returns>
    public static bool IsValidInternalId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        int index = value.IndexOf('-');
        if (index >= 0)
        {
            value = value.Remove(index, 1);
        }
        else
        {
            return false;
        }

        return HasValidChecksum(value);
    }

    private static bool HasValidChecksum(string digits)
    {
        if (digits.Length != 15 || !digits.All(char.IsDigit))
        {
            return false;
        }

        int sum = 0;

        for (int i = 0; i < digits.Length - 1; i++)
        {
            int digit = digits[i] - '0';
            int weight = (i % 2 == 0) ? 1 : 3;
            sum += digit * weight;
        }

        int checksum = sum % 10;
        int controlDigit = digits[^1] - '0';

        return checksum == controlDigit;
    }
}
