using System.Text.RegularExpressions;

namespace PMMS.Server.Common.Helper;

public static partial class StringFormatter
{
    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex CamelCaseRegex();

    public static string SplitCamelCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return CamelCaseRegex().Replace(input, " ");
    }
}