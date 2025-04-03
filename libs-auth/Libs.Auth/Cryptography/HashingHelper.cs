using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Libs.Auth.Cryptography;

public static class HashingHelper
{

    public static byte[] GenerateSalt()
    {
        var salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    public static string HashPassword(string senha, byte[] salt)
    {
        using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(senha)))
        {
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8; // Número de threads
            argon2.MemorySize = 65536;      // Memória usada (64 MB)
            argon2.Iterations = 4;          // Número de iterações

            byte[] hash = argon2.GetBytes(32); // Gera um hash de 32 bytes
            return Convert.ToBase64String(hash);
        }
    }
}