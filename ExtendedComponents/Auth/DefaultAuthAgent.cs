using System.Text.RegularExpressions;
using Auth;
using ExtendedComponents.Core;

namespace ExtendedComponents.Auth;

public class DefaultAuthAgent : AuthAgent
{
    public DefaultAuthAgent(AuthQuery query, AuthUserGenerator userGen, AuthTokenProvider tokenizer) : base(query, userGen, tokenizer)
    {
        
    }

    public override AuthToken SignupWithEmailPassword(AuthRequest request)
    {
        var passwordMinLen = ProjectSettings.Instance.Get(SettingsCatalog.AuthPasswordMinimumLength, 6);
        AuthToken token = Query.Write(authQuery =>
        {
            string email = request.username.ToLower();
            string password = request.password;
            if (authQuery.Has(email))
            {
                throw new EmailExistedException();
            }
            if (password.Length < passwordMinLen)
            {
                throw new InvalidPasswordException();
            }
            if (!Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            {
                throw new MalformedEmailException();
            }

            AuthUser user = UserGen.CreateUserPassword(new UserGenRequest
            {
                UserName = email, Password = password
            });
            var re = SignupSuccessAction?.Invoke(user);
            if (re == SignupIntervention.Reject) throw new SignupRejectedException();
            authQuery.ById[email] = user;
            return Tokenizer.GenerateToken(new AuthTokenInfo
            {
                TargetUser = user.UserUuid,
            });
        });
        return token;
    }

    public override AuthToken LoginWithEmailPassword(AuthRequest request)
    {
        string email = request.username.ToLower();
        string password = request.password;
        AuthUser? user;
        try
        {
            user = Query.Read(authQuery => authQuery.ById[email]);
        }
        catch (Exception)
        {
            user = null;
        }
        if (user == null) throw new InvalidLoginCredentialException();
        if (!UserGen.VerifyUserPassword(user, password))
        {
            throw new InvalidLoginCredentialException();
        }
        AuthToken token = Tokenizer.GenerateToken(new AuthTokenInfo
        {
            TargetUser = user.UserUuid
        });
        return token;
    }

    public override AuthToken RefreshAuthToken(AuthToken refreshToken)
    {
        AuthTokenInfo originalToken = Tokenizer.RetrieveToken(refreshToken);
        return Tokenizer.GenerateToken(new AuthTokenInfo
        {
            TargetUser = originalToken.TargetUser
        });
    }

    public override AuthUser GetUserByIdToken(AuthToken token)
    {
        AuthTokenInfo originalToken = Tokenizer.RetrieveToken(token);
        try
        {
            return Query.Read((authQuery) => authQuery.ById[originalToken.TargetUser]);
        }
        catch (Exception)
        {
            throw new InvalidLoginCredentialException();
        }
    }

    public override AuthUser GetUserByUUID(string uuid)
    {
        try
        {
            return Query.Read((authQuery) => authQuery.ByUuid[uuid]);
        }
        catch (Exception)
        {
            throw new InvalidLoginCredentialException();
        }
    }

    public override bool IsAuthTokenValid(AuthToken token)
    {
        try
        {
            AuthTokenInfo originalToken = Tokenizer.RetrieveToken(token);
            if (DateTimeOffset.Now.ToUnixTimeSeconds() - originalToken.ActivatedSince >=
                ProjectSettings.Instance.Get(SettingsCatalog.AuthTokenExpirationHour, 0.5) * 60 * 60)
                throw new InvalidTokenException();
            return Query.Read(authQuery => authQuery.Has(originalToken.TargetUser));
        }
        catch (Exception)
        {
            return false;
        }
    }

    public override string GetUserIdFromToken(AuthToken token)
    {
        if (!IsAuthTokenValid(token)) throw new InvalidTokenException();
        AuthTokenInfo originalToken = Tokenizer.RetrieveToken(token);
        return originalToken.TargetUser;
    }
}