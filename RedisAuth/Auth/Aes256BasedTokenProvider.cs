using System.Text;
using Auth;
using CoreComponents.Core;
using Newtonsoft.Json;

namespace RedisAuth.Auth;

public class Aes256BasedTokenProvider : AuthTokenProvider
{
    private readonly Aes256Provider _provider;
    public Aes256BasedTokenProvider(string secret)
    {
        _provider = new(Encoding.UTF8.GetBytes(secret));
    }
    public override AuthToken GenerateToken(AuthTokenInfo request)
    {
        var asString = JsonConvert.SerializeObject(request);
        return new AuthToken
        {
            IdToken = Uri.EscapeDataString(_provider.Encrypt(asString))
        };
    }

    public override AuthTokenInfo RetrieveToken(AuthToken token)
    {
        try
        {
            var asString = _provider.Decrypt(Uri.UnescapeDataString(token.IdToken));
            var asObject = JsonConvert.DeserializeObject<AuthTokenInfo>(asString);
            if (asObject == null) throw new InvalidTokenException();
            return asObject;
        }
        catch (Exception)
        {
            throw new InvalidTokenException();
        }
    }
}