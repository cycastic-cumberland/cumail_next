using System.Security.Cryptography;
using System.Text;

namespace CumailNEXT.Components.Core;

public class Aes256Provider
{
    private readonly byte[] key;
    private readonly byte[] iv;
    private readonly Aes aes;
    public Aes256Provider(byte[] key, byte[] iv)
    {
        this.key = key;
        this.iv = iv;
        
        aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Padding = PaddingMode.Zeros;

        aes.Key = key;
        aes.IV = iv;
    }

    private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
    {
        using var ms = new MemoryStream();
        using var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();

        return ms.ToArray();
    }
    public byte[] Encrypt(string text)
    {
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        return PerformCryptography(Encoding.ASCII.GetBytes(text), encryptor);
    }

    public string EncryptBase64(string text)
    {
        return (Convert.ToBase64String(Encrypt(text)));
    }

    public string Decrypt(byte[] cipher)
    {
        // foreach (var c in cipher)
        // {
        //     Console.Write(c);
        // }
        // Console.WriteLine();
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var raw = PerformCryptography(cipher, decryptor);
        return Encoding.ASCII.GetString(raw);
    }

    public string? DecryptBase64Safe(string base64Text)
    {
        try
        {
            return Decrypt(Convert.FromBase64String((base64Text)));
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}