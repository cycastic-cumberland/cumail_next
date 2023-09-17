using ChatApp.Schemas;
using ChatApp.Tables;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDChatApp.ChatApp.Schemas;

public class MChatRoomPersonnel : MDataclass<IChatRoomPersonnel>
{
    [BsonId]
    [BsonElement("chat_room_id")]
    public string ChatRoomId { get; set; } = "";
    
    [BsonElement("user_id")]
    public string UserId { get; set; } = "";
    
    [BsonElement("role")]
    public int Role { get; set; }
    public override IChatRoomPersonnel ToCommon()
    {
        return new ChatRoomPersonnel()
        {
            ChatRoomId = ChatRoomId,
            UserId = UserId,
            Role = Role,
        };
    }

    public override void FromCommon(IChatRoomPersonnel common)
    {
        ChatRoomId = common.GetRoomId();
        UserId = common.GetUserId();
        Role = common.GetRole();
    }
    
    public static MChatRoomPersonnel Marshall(IChatRoomPersonnel common)
        => Marshall<MChatRoomPersonnel>(common);
}