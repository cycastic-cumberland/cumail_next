namespace Auth;

public class AuthTokenInfo
{
    public string TargetUser = "";
    public long ActivatedSince = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public string AuthorizedBy = "";

}

public class AuthToken
{
    public string IdToken { get; set; } = "";
    public AuthToken() {}

    public AuthToken(string token)
    {
        IdToken = token;
    }
}