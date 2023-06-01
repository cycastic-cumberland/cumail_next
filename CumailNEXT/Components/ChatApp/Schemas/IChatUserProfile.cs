namespace CumailNEXT.Components.ChatApp.Schemas;

public interface IChatUserProfile
{
    public string GetUserId();
    public string GetUserName();
    public string GetPfpUrl();
    public string GetDescription();
    public bool IsDisabled();
    public bool IsDeleted();
}