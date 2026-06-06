using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Accessly.Domain.Common;

/// <summary>Produces URL-friendly slugs from arbitrary text.</summary>
public static partial class SlugGenerator
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        var slug = builder.ToString().Normalize(NormalizationForm.FormC);
        slug = NonAlphanumeric().Replace(slug, "-");
        slug = MultipleHyphens().Replace(slug, "-");
        return slug.Trim('-');
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleHyphens();
}
