using Auth;
using ExtendedComponents.Auth;

namespace RedisAuth.Auth;

public class ModularAuthProvider : TokenBasedAuthProvider<DefaultAuthAgent>
{
    public ModularAuthProvider(AuthQuery query, string tokenSecret) 
        : base(new DefaultAuthAgent(query,
            new BCryptUserGen(),
            new JwtTokenProvider(tokenSecret)))
    {
    }

    public override void OnSignupSuccess(Func<AuthUser, SignupIntervention> action)
    {
        Agent.OnSignupSuccess(action);
    }
}