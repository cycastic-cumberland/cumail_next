namespace CumailNEXT.Components.Auth;

public class TokenBasedAuthProvider<TAuthAgent> : AuthProvider
    where TAuthAgent : AuthAgent
{
    protected readonly TAuthAgent AuthAgent;
        
    public TokenBasedAuthProvider(TAuthAgent authAgent)
    {
        AuthAgent = authAgent;
    }
    public override AuthToken SignupWithEmailPassword(AuthRequest request)
    {
        return AuthAgent.SignupWithEmailPassword(request);
    }

    public override AuthToken LoginWithEmailPassword(AuthRequest request)
    {
        return AuthAgent.LoginWithEmailPassword(request);
    }

    public override AuthToken RefreshAuthToken(AuthToken refreshToken)
    {
        return AuthAgent.RefreshAuthToken(refreshToken);
    }

    public override AuthUser GetUserByIdToken(AuthToken token)
    {
        return AuthAgent.GetUserByIdToken(token);
    }

    public override AuthUser GetUserByUUID(string uuid)
    {
        return AuthAgent.GetUserByUUID(uuid);
    }

    public override bool IsAuthTokenValid(AuthToken token)
    {
        return AuthAgent.IsAuthTokenValid(token);
    }

    public override string GetUserIdFromToken(AuthToken token)
    {
        return AuthAgent.GetUserIdFromToken(token);
    }
}