namespace Auth;

public class UserGenRequest
{
    public string UserName = "";
    public string Password = "";
}

public abstract class AuthUserGenerator
{
    public abstract AuthUser CreateUserPassword(UserGenRequest request);
    public abstract bool VerifyUserPassword(AuthUser user, string password);
}