using System.Text.Json.Serialization;

namespace CumailNEXT.Components.Auth;

public abstract class AuthTokenProvider
{
    public abstract AuthToken GenerateToken(AuthTokenInfo request);
    public abstract AuthTokenInfo RetrieveToken(AuthToken token);
}