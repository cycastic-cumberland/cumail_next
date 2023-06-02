using CumailNEXT.Components.Auth;
using CumailNEXT.Implementation.Database;

namespace CumailNEXT.Implementation.Auth;

public class ModularAuthProvider : TokenBasedAuthProvider<DefaultAuthAgent>
{
    public ModularAuthProvider(RedisProvider mainDb, string tokenSecret) 
        : base(new DefaultAuthAgent(new MonolithicRedisAuthQuery(mainDb),
            new BCryptUserGen(),
            new JwtTokenProvider(tokenSecret)))
    {
    }

    public override void OnSignupSuccess(Func<AuthUser, SignupIntervention> action)
    {
        Agent.OnSignupSuccess(action);
    }
}