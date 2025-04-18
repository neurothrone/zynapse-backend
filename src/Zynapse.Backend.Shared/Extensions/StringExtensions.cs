namespace Zynapse.Backend.Shared.Extensions;

public static class StringExtensions
{
    public static string Capitalize(this string input) => string.IsNullOrEmpty(input)
        ? input
        : char.ToUpper(input[0]) + input[1..].ToLower();
}