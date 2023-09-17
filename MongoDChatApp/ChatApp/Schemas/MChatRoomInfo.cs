using ChatApp.Schemas;
using ChatApp.Tables;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDChatApp.ChatApp.Schemas;

public class MChatRoomInfo : MDataclass<IChatRoomProfile>
{
    [BsonId]
    [BsonElement("room_id")]
    public string RoomId { get; set; } = "";
    
    [BsonElement("room_name")]
    public string RoomName { get; set; } = "";
    
    [BsonElement("description")]
    public string Description { get; set; } = "";
    
    [BsonElement("created_at")]
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    
    [BsonElement("last_activity")]
    public long LastActivity { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    
    [BsonElement("is_public")]
    public bool IsPublic { get; set; } = false;
    
    [BsonElement("hashed_password")]
    public string HashedPassword { get; set; } = "";
    
    [BsonElement("max_user")]
    public int MaxUsers { get; set; } = 50;

    public override IChatRoomProfile ToCommon()
    {
        return new ChatRoomInfo()
        {
            RoomId = RoomId,
            RoomName = RoomName,
            Description = Description,
            CreatedAt = CreatedAt,
            LastActivity = LastActivity,
            IsPublic = IsPublic,
            HashedPassword = HashedPassword,
            MaxUsers = MaxUsers,
        };
    }

    public override void FromCommon(IChatRoomProfile common)
    {
        RoomId = common.GetRoomId();
        RoomName = common.GetRoomName();
        Description = common.GetDescription();
        CreatedAt = common.GetCreationTime();
        LastActivity = common.GetLastActivity();
        IsPublic = common.IsVisible();
        HashedPassword = common.GetHashedPassword();
        MaxUsers = common.GetMaxUsers();
    }

    public static MChatRoomInfo Marshall(IChatRoomProfile common)
        => Marshall<MChatRoomInfo>(common);
}