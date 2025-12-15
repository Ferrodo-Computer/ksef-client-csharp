using System.Reflection;
using System.Text.RegularExpressions;

namespace KSeF.Client.Validation;
/// <summary>
/// Zapewnia metodę do walidacji obiektów zawierających właściwości „Type” (enum) oraz „Value” (string)
/// przy użyciu wzorców wyrażeń regularnych zdefiniowanych w klasie <c>RegexPatterns</c>.
/// </summary>
public static class TypeValueValidator
{
    /// <summary>
    /// Weryfikuje, czy przekazany obiekt ma prawidłowo ustawione właściwości „Type” i „Value”
    /// zgodnie z regułami zdefiniowanymi dla danego typu oraz powiązanym wzorcem wyrażenia regularnego.
    /// </summary>
    /// <param name="objectToValidate">
    /// Obiekt do walidacji, który musi posiadać publiczne właściwości <c>Type</c> (typu <see cref="Enum"/>)
    /// oraz <c>Value</c> (typu <see cref="string"/>).
    /// </param>
    /// <returns>
    /// Wartość <see langword="true"/>, jeśli obiekt spełnia reguły walidacji,
    /// w przeciwnym razie <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Rzucany, gdy:
    /// - obiekt nie posiada właściwości <c>Type</c>,
    /// - obiekt nie posiada właściwości <c>Value</c>,
    /// - właściwość <c>Type</c> nie jest typu wyliczeniowego (<see cref="Enum"/>).
    /// </exception>
    /// <remarks>
    /// Logika walidacji:
    /// - Dla typu <c>AllPartners</c> wartość <c>Value</c> musi być pusta (lub <see langword="null"/>),
    /// - Dla pozostałych typów wymagana jest niepusta wartość <c>Value</c>,
    /// - Jeśli w klasie <c>RegexPatterns</c> istnieje publiczna statyczna właściwość o nazwie odpowiadającej nazwie typu,
    /// jej wartość traktowana jest jako wzorzec <see cref="Regex"/> i używana do walidacji <c>Value</c>,
    /// - Jeśli wzorzec dla danego typu nie istnieje, wartość <c>Value</c> uznawana jest za poprawną.
    /// </remarks>
    public static bool Validate(object objectToValidate)
    {
        Type objectType = objectToValidate.GetType();

        PropertyInfo propertyTypeInfo = objectType.GetProperty("Type");
        PropertyInfo propertyValueInfo = objectType.GetProperty("Value");

        if (propertyTypeInfo is null)
        {
            throw new ArgumentException($"Właściwość 'Type' nie została znaleziona w obiekcie typu `{objectType.Name}.");
        }
        if (propertyValueInfo is null)
        {
            throw new ArgumentException($"Właściwość 'Value' nie została znaleziona w obiekcie typu `{objectType.Name}.");
        }
        if (propertyTypeInfo.GetValue(objectToValidate) is not Enum type)
        {
            throw new ArgumentException($"Właściwość 'Type' w obiekcie typu `{objectType.Name}` nie jest typu Enum.");
        }

        string valueToValidate = propertyValueInfo.GetValue(objectToValidate) as string;

        if (type.ToString() == "AllPartners")
        {
            if (string.IsNullOrEmpty(valueToValidate))
            {
                return true; // Dla typu AllPartners wartość 'Value' musi być pusta
            }
            else
            {
                return false;
            }
        }

        Type regexPatterns = typeof(RegexPatterns);

        PropertyInfo regexInfo = regexPatterns.GetProperty(type.ToString(), BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        if (regexInfo is null)
        {
            return true; // Brak wzorca regex dla danego typu, wartość uznawana za poprawną
        }
        if (string.IsNullOrEmpty(valueToValidate))
        {
            return false;
        }

        Regex regexPattern = regexInfo.GetValue(null) as Regex;

        return regexPattern.IsMatch(valueToValidate);
    }
}

