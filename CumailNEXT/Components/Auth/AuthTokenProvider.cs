using System.Text;
using System.Text.Json.Serialization;
using CumailNEXT.Components.Core;
using Newtonsoft.Json;

namespace CumailNEXT.Components.Auth;

public class Aes256BasedTokenProvider : AuthTokenProvider
{
    private readonly Aes256Provider _cipherAgent;
    public Aes256BasedTokenProvider(string secret)
    {
        _cipherAgent = new Aes256Provider((Encoding.ASCII.GetBytes(secret)), Crypto.GenerateSecureBytes(16));
    }
    public override AuthToken GenerateToken(AuthTokenInfo request)
    {
        var serialized = JsonConvert.SerializeObject(request);
        // serialized = System.Net.WebUtility.UrlEncode(serialized);
        var encrypted = _cipherAgent.EncryptBase64(serialized);

        var decrypted = _cipherAgent.DecryptBase64Safe(encrypted);
        
        return new AuthToken
        {
            IdToken = encrypted
        };
    }

    public override AuthTokenInfo RetrieveToken(AuthToken token)
    {
        var decrypted = _cipherAgent.DecryptBase64Safe(token.IdToken);
        if (decrypted == null) throw new InvalidTokenException();
        // decrypted = System.Net.WebUtility.UrlDecode(decrypted);
        var tokenInfo = JsonConvert.DeserializeObject<AuthTokenInfo>(decrypted);
        if (tokenInfo == null) throw new InvalidTokenException();
        return tokenInfo;
    }
}

public abstract class AuthTokenProvider
{
    public abstract AuthToken GenerateToken(AuthTokenInfo request);
    public abstract AuthTokenInfo RetrieveToken(AuthToken token);
}