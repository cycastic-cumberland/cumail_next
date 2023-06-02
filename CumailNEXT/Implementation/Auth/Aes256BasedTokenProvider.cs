using System.Text;
using CumailNEXT.Components.Auth;
using CumailNEXT.Components.Core;
using CumailNEXT.Implementation.Core;
using Newtonsoft.Json;

namespace CumailNEXT.Implementation.Auth;

public class Aes256BasedTokenProvider : AuthTokenProvider
{
    private readonly Aes256Provider _provider;
    public Aes256BasedTokenProvider(string secret)
    {
        _provider = new(Encoding.UTF8.GetBytes(secret), Engine.TokenInitVector);
    }
    public override AuthToken GenerateToken(AuthTokenInfo request)
    {
        var asString = JsonConvert.SerializeObject(request);
        return new AuthToken
        {
            IdToken = _provider.Encrypt(asString)
        };
    }

    public override AuthTokenInfo RetrieveToken(AuthToken token)
    {
        try
        {
            var asString = _provider.Decrypt(token.IdToken);
            var asObject = JsonConvert.DeserializeObject<AuthTokenInfo>(asString);
            if (asObject == null) throw new InvalidTokenException();
            return asObject;
        }
        catch (Exception)
        {
            throw new InvalidTokenException();
        }
    }

    public override void Dispose()
    {
        _provider.Dispose();
    }
}