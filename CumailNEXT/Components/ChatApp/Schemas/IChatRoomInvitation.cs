namespace CumailNEXT.Components.ChatApp.Schemas;

public interface IChatRoomInvitation
{
    public string GetInvitationId();
    public string GetRoomId();
    public string GetInstigatorId();
    public bool IsEnabled();
    
    // Sensitive contents
    public void SetInstigatorId(string newId)
    {
    }

    public void SetEnabled(bool newState)
    {
        
    }
}