using CumailNEXT.Components.ChatApp.Schemas;

namespace CumailNEXT.Implementation.ChatApp.Tables;

public class PartialInvitation : DictionaryPartialDataclass, IChatRoomInvitation
{
    public PartialInvitation(Dictionary<string, object> anonymousObject) : base(anonymousObject)
    {
    }

    public string GetInvitationId() => Fetch<string>("InvitationString");
    public string GetRoomId() => Fetch<string>("ChatRoomId");
    public string GetInstigatorId() => Fetch<string>("InstigatorId");
    public bool IsEnabled() => FetchNative<bool>("IsEnabled");
}

public class ChatRoomInvitation : IChatRoomInvitation
{
    public string InvitationString { get; set; } = "";
    public string InstigatorId { get; set; } = "";
    public string ChatRoomId { get; set; } = "";
    public bool IsEnabled { get; set; } = false;
    
    public string GetInvitationId()
    {
        return InvitationString;
    }

    public string GetRoomId()
    {
        return ChatRoomId;
    }

    public string GetInstigatorId()
    {
        return InstigatorId;
    }

    bool IChatRoomInvitation.IsEnabled()
    {
        return IsEnabled;
    }

    public void SetInstigatorId(string newId)
    {
        InstigatorId = newId;
    }

    public void SetEnabled(bool newState)
    {
        IsEnabled = newState;
    }
}