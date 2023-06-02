namespace CumailNEXT.Components.Auth;

public abstract class AuthTokenProvider : IDisposable
{
    public abstract AuthToken GenerateToken(AuthTokenInfo request);
    public abstract AuthTokenInfo RetrieveToken(AuthToken token);
    public virtual void Dispose(){}
}