namespace smart_home_server.Utils;

using System.Security.Cryptography;
using System.Text;

public class PasswordHasher
{
    public static HashAlgorithmName Algorithm = HashAlgorithmName.SHA1;
    public static int HashSize = 160;
    public static int SaltSize = 16;
    public static int Iteration = 8192;
    public static string HashPassword(string password)
    {
        byte[] saltBuffer;
        byte[] hashBuffer;

        using (var keyDerivation = new Rfc2898DeriveBytes(password, SaltSize, Iteration, Algorithm))
        {
            saltBuffer = keyDerivation.Salt;
            hashBuffer = keyDerivation.GetBytes(SaltSize);
        }

        byte[] result = new byte[HashSize + SaltSize];
        Buffer.BlockCopy(hashBuffer, 0, result, 0, SaltSize);
        Buffer.BlockCopy(saltBuffer, 0, result, HashSize, SaltSize);
        return Convert.ToBase64String(result);
    }

    public static string CreateMd5(string password)
    {
        using (var md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(password);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}
