using Domain.Exceptions;

namespace Domain.CatalogManagement.ValueObjects;

public sealed class Isbn : IEquatable<Isbn>
{
    private readonly string _value;

    private Isbn(string value) => _value = value;

    public static Isbn Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)){
            throw new DomainValidationException("ISBN não pode ser vazio");
        }

        var cleanValue = value.Replace("-", "").Replace(" ", "");

        if (!IsValidIsbn(cleanValue)){
            throw new DomainValidationException("ISBN inválido");
        }

        return new Isbn(cleanValue);
    }

    private static bool IsValidIsbn(string value)
    {
        if (value.Length == 10)
        {
            return ValidateIsbn10(value);
        }

        if (value.Length == 13)
        {
            return ValidateIsbn13(value);
        }

        return false;
    }

    private static bool ValidateIsbn10(string value)
    {
        try
        {
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                int digit = int.Parse(value[i].ToString());
                sum += (digit * (10 - i));
            }

            char lastChar = value[9];
            int checksum = (lastChar == 'X') ? 10 : int.Parse(lastChar.ToString());
            sum += checksum;

            return sum % 11 == 0;
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
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = int.Parse(value[i].ToString());
                sum += (digit * (i % 2 == 0 ? 1 : 3));
            }

            int checksum = int.Parse(value[12].ToString());
            int calculatedChecksum = (10 - (sum % 10)) % 10;

            return checksum == calculatedChecksum;
        }
        catch
        {
            return false;
        }
    }

    public string FormattedIsbn()
    {
        if (_value.Length == 10)
            return $"{_value[0]}-{_value.Substring(1, 4)}-{_value.Substring(5, 4)}-{_value[9]}";

        return $"{_value.Substring(0, 3)}-{_value[3]}-{_value.Substring(4, 4)}-{_value.Substring(8, 4)}-{_value[12]}";
    }

    public override string ToString() => _value;

    // Implementação de igualdade (Value Object)
    public bool Equals(Isbn? other) => other != null && _value == other._value;
    public override bool Equals(object? obj) => obj is Isbn other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();

    public static implicit operator string(Isbn isbn) => isbn._value;
}