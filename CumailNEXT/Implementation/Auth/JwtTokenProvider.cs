using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CumailNEXT.Components.Auth;
using CumailNEXT.Components.Core;
using CumailNEXT.Implementation.Core;
using Microsoft.IdentityModel.Tokens;

namespace CumailNEXT.Implementation.Auth;

public class JwtTokenProvider : AuthTokenProvider
{
    private readonly JwtSecurityTokenHandler _handler;
    private readonly SymmetricSecurityKey _key;
    
    public JwtTokenProvider(string secret)
    {
        _handler = new JwtSecurityTokenHandler();
        _key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
    }
    public override AuthToken GenerateToken(AuthTokenInfo request)
    {
        var expiration = ProjectSettings.Instance.Get(SettingsCatalog.AuthTokenExpirationHour, 6);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, request.TargetUser)
            }),
            Expires = DateTime.Now.AddHours(expiration),
            Issuer = ProjectSettings.Instance.Get(SettingsCatalog.AuthIssuer, ""),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
        };
        var token = _handler.CreateToken(tokenDescriptor);
        return new AuthToken
        {
            IdToken = _handler.WriteToken(token)
        };
    }

    public override AuthTokenInfo RetrieveToken(AuthToken token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key
        };
        var principal = _handler.ValidateToken(token.IdToken, validationParameters, out var validatedToken);
        if (principal.Identity == null) throw new SecurityTokenException();
        return new AuthTokenInfo
        {
            TargetUser = principal.Identity.Name ?? "",
            ActivatedSince = new DateTimeOffset(validatedToken.ValidFrom).ToUnixTimeSeconds(),
            AuthorizedBy = validatedToken.Issuer ?? ""
        };
    }
}