using ChatApp.Schemas;

namespace ChatApp.Tables;

public class PartialPersonnel : DictionaryPartialDataclass, IChatRoomPersonnel
{
    public PartialPersonnel(Dictionary<string, object> anonymousObject) : base(anonymousObject)
    {
    }

    public string GetUserId() => Fetch<string>("UserId");

    public string GetRoomId() => Fetch<string>("ChatRoomId");

    public int GetRole() => FetchNative<int>("Role");
}

public class ChatRoomPersonnel : IChatRoomPersonnel
{
    public string ChatRoomId { get; set; } = "";
    public string UserId { get; set; } = "";
    public int Role { get; set; }

    public string GetUserId()
    {
        return UserId;
    }

    public string GetRoomId()
    {
        return ChatRoomId;
    }

    public int GetRole()
    {
        return Role;
    }
}