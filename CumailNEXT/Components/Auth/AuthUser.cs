namespace CumailNEXT.Components.Auth;

public class UuidReference
{
    public string UserUuid { get; set; } = "";
}

public class AuthUser
{
    public string UserLoginKey { get; set; } = "";
    public string HashedPassword { get; set; } = "";
    public string UserUuid { get; set; } = "";
    public long CreationTime { get; set; } = 0;
}