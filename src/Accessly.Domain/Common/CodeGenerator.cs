using System.Security.Cryptography;

namespace Accessly.Domain.Common;

/// <summary>Generates short, unambiguous, uppercase codes for tickets.</summary>
public static class CodeGenerator
{
    // Excludes easily-confused characters (0/O, 1/I, etc.).
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(int length = 10)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }

        return new string(chars);
    }
}
