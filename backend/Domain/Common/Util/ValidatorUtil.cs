namespace Domain.Common.Util;

public static class ValidatorUtil
{
    public static bool IsValidGuid(string guidString)
    {
        return Guid.TryParse(guidString, out _);
    }
}