// src/Common/Security/PasswordHasher.cs
using System.Security.Cryptography;
using System.Text;

namespace Common.Security
{
    public static class PasswordHasher
    {
        /// <summary>
        /// Calcula o SHA256 em hex minúsculo (compatível com o que usamos na API).
        /// </summary>
        public static string Hash(string input)
        {
            if (input == null) input = string.Empty;

            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.Append(b.ToString("x2")); // hex minúsculo

            return sb.ToString();
        }
    }
}
