using System.Security.Cryptography;
using System.Text;

namespace CumailNEXT.Components.Core;

public class Aes256Provider
{
    private readonly byte[] _secret;
    
    public Aes256Provider(byte[] secret)
    {
        if (secret.Length != 32) throw new InvalidDataException("Incorrect bytes length");
        _secret = secret;
    }
    public string Encrypt(string plainText)
    {
        byte[] encryptedBytes;
        using (var aes = Aes.Create())
        {
            aes.Key = _secret;
            aes.IV = Crypto.GenerateSecureBytes(16);
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
            encryptedBytes = aes.IV.Concat(encryptedBytes).ToArray();
        }

        return Convert.ToBase64String(encryptedBytes);
    }
    public string Decrypt(string encryptedText)
    {
        byte[] decryptedBytes;
        using (var aes = Aes.Create())
        {
            var fullBytes = Convert.FromBase64String(encryptedText);
            var iv = fullBytes[..16];
            var encryptedBytes = fullBytes[16..];

            aes.Key = _secret;
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            
        }
        
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}