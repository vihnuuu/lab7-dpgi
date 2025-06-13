using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    public static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            StringBuilder builder = new StringBuilder();
            foreach (var t in bytes)
                builder.Append(t.ToString("x2"));

            return builder.ToString();
        }
    }

    public static bool VerifyPassword(string inputPassword, string storedHash)
    {
        string inputHash = ComputeSha256Hash(inputPassword);
        return inputHash == storedHash;
    }
}
