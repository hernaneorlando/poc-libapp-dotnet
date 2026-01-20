namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value Object for ISBN validation.
/// Supports both ISBN-10 and ISBN-13 formats.
/// </summary>
public sealed class IsbnNumber : ValueObject
{
    public string Value { get; }

    private IsbnNumber(string value) => Value = value;

    public static IsbnNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ISBN cannot be empty");

        var cleanValue = value.Replace("-", "").Replace(" ", "");

        if (!IsValidIsbn(cleanValue))
            throw new ArgumentException($"Invalid ISBN format: {value}");

        return new IsbnNumber(cleanValue);
    }

    private static bool IsValidIsbn(string value)
    {
        return value.Length == 10 
            ? ValidateIsbn10(value) 
            : value.Length == 13 && ValidateIsbn13(value);
    }

    private static bool ValidateIsbn10(string value)
    {
        try
        {
            if (!value.All(c => char.IsDigit(c) || c == 'X'))
                return false;

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                if (!int.TryParse(value[i].ToString(), out int digit))
                    return false;
                sum += digit * (10 - i);
            }

            int checkDigit = 11 - (sum % 11);
            if (checkDigit == 10)
                return value[9] == 'X';
            if (checkDigit == 11)
                return value[9] == '0';

            return int.TryParse(value[9].ToString(), out int lastDigit) && checkDigit == lastDigit;
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidateIsbn13(string value)
    {
        try
        {
            if (!value.All(char.IsDigit))
                return false;

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                if (!int.TryParse(value[i].ToString(), out int digit))
                    return false;
                sum += i % 2 == 0 ? digit : digit * 3;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return int.TryParse(value[12].ToString(), out int lastDigit) && checkDigit == lastDigit;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(IsbnNumber isbn) => isbn.Value;

    public override string ToString() => Value;
}
