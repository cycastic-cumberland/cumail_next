using System.Security.Cryptography;
using System.Text;

namespace CumailNEXT.Components.Core;

public class Aes256Provider : IDisposable
{
    private readonly Aes _provider;
    
    public Aes256Provider(byte[] secret)
        : this(secret, Crypto.GenerateSecureBytes(16))
    {
    }
    public Aes256Provider(byte[] secret, byte[] iv)
    {
        if (secret.Length != 32 || iv.Length != 16) throw new InvalidDataException("Incorrect bytes length");
        _provider = Aes.Create();
        _provider.Key = secret;
        _provider.IV = iv;
    }
    public string Encrypt(string plainText)
    {
        var encryptor = _provider.CreateEncryptor(_provider.Key, _provider.IV);
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }
    public string Decrypt(string encryptedText)
    {
        var decryptor = _provider.CreateDecryptor(_provider.Key, _provider.IV);
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
}