namespace Domain.SeedWork.Common.Util;

public static class StringExtensions
{
    public static string ToSnakeCaseFast(this string input)
{
    if (string.IsNullOrEmpty(input))
        return input;

    int bufferLength = input.Length * 2;
    var buffer = new char[bufferLength];
    int bufferPosition = 0;
    bool lastWasUpper = false;

    for (int i = 0; i < input.Length; i++)
    {
        char c = input[i];
        if (char.IsUpper(c))
        {
            if (i > 0 && !lastWasUpper && bufferPosition < bufferLength - 1)
            {
                buffer[bufferPosition++] = '_';
            }
            buffer[bufferPosition++] = char.ToLowerInvariant(c);
            lastWasUpper = true;
        }
        else
        {
            buffer[bufferPosition++] = c;
            lastWasUpper = false;
        }
    }

    return new string(buffer, 0, bufferPosition);
}
}