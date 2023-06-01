using System.Security.Cryptography;
using BC = BCrypt.Net.BCrypt;
using System.Text;
using CumailNEXT.Components.Auth;
using CumailNEXT.Components.Core;
using CumailNEXT.Implementation.Core;

namespace CumailNEXT.Implementation.Auth;

public class BCryptUserGen : AuthUserGenerator
{
    private readonly int _saltRound = ProjectSettings.Instance.Get(SettingsCatalog.AuthPasswordSaltRound, 12);
    
    public override AuthUser CreateUserPassword(UserGenRequest request)
    {
        AuthUser re = new AuthUser
        {
            UserLoginKey = request.UserName,
            CreationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            HashedPassword = BC.HashPassword(request.Password, workFactor: _saltRound)
        };
        re.UserUuid = Crypto.HashSha256String($"key:{re.UserLoginKey};time:{re.CreationTime}");
        return re;
    }

    public override bool VerifyUserPassword(AuthUser user, string password)
    {
        return BC.Verify(password, user.HashedPassword);
    }
}