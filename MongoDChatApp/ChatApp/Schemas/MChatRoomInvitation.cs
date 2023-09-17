using ChatApp.Schemas;
using ChatApp.Tables;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDChatApp.ChatApp.Schemas;

public class MChatRoomInvitation : MDataclass<IChatRoomInvitation>
{
    [BsonId]
    [BsonElement("invitation_string")]
    public string InvitationString { get; set; } = "";
    
    [BsonElement("instigator_id")]
    public string InstigatorId { get; set; } = "";
    
    [BsonElement("chat_room_id")]
    public string ChatRoomId { get; set; } = "";
    
    [BsonElement("is_enabled")]
    public bool IsEnabled { get; set; } = false;
    
    public override IChatRoomInvitation ToCommon()
    {
        return new ChatRoomInvitation()
        {
            InvitationString = InvitationString,
            InstigatorId = InstigatorId,
            ChatRoomId = ChatRoomId,
            IsEnabled = IsEnabled,
        };
    }

    public override void FromCommon(IChatRoomInvitation common)
    {
        InvitationString = common.GetInvitationId();
        InstigatorId = common.GetInstigatorId();
        ChatRoomId = common.GetRoomId();
        IsEnabled = common.IsEnabled();
    }

    public static MChatRoomInvitation Marshall(IChatRoomInvitation common)
        => Marshall<MChatRoomInvitation>(common);
}