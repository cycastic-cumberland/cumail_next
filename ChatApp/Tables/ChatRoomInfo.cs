using ChatApp.Schemas;

namespace ChatApp.Tables;

public class PartialChatRoomProfile : PartialDataclass, IChatRoomProfile
{

    public string GetRoomId() => Fetch<string>("RoomId");
    public string GetRoomName() => Fetch<string>("RoomName");
    public string GetDescription() => Fetch<string>("Description");
    public long GetCreationTime() => FetchNative<long>("CreatedAt");
    public long GetLastActivity() => FetchNative<long>("LastActivity");
    public bool IsVisible() => FetchNative<bool>("IsPublic");
    public string GetHashedPassword() => Fetch<string>("HashedPassword");
    public int GetMaxUsers() => FetchNative<int>("MaxUsers");

    public PartialChatRoomProfile(object anonymousObject) : base(anonymousObject)
    {
    }
}

public class PartialChatRoomProfileDict : DictionaryPartialDataclass, IChatRoomProfile
{
    public PartialChatRoomProfileDict(Dictionary<string, object> anonymousObject) : base(anonymousObject)
    {
    }

    public string GetRoomId() => Fetch<string>("RoomId");
    public string GetRoomName() => Fetch<string>("RoomName");
    public string GetDescription() => Fetch<string>("Description");
    public long GetCreationTime() => FetchNative<long>("CreatedAt");
    public long GetLastActivity() => FetchNative<long>("LastActivity");
    public bool IsVisible() => FetchNative<bool>("IsPublic");
    public string GetHashedPassword() => Fetch<string>("HashedPassword");
    public int GetMaxUsers() => FetchNative<int>("MaxUsers");
}

public class ChatRoomInfo : IChatRoomProfile
{
    public string RoomId { get; set; } = "";
    public string RoomName { get; set; } = "";
    public string Description { get; set; } = "";
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public long LastActivity { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public bool IsPublic { get; set; } = false;
    public string HashedPassword { get; set; } = "";
    public int MaxUsers { get; set; } = 50;
    public string GetRoomId() => RoomId;

    public string GetRoomName() => RoomName;

    public string GetDescription() => Description;

    public long GetCreationTime() => CreatedAt;

    public long GetLastActivity() => LastActivity;

    public bool IsVisible() => IsPublic;

    public string GetHashedPassword() => HashedPassword;

    public int GetMaxUsers() => MaxUsers;
    public void SetHashedPassword(string newPassword)
    {
        HashedPassword = newPassword;
    }
}