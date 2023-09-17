using ChatApp;
using ChatApp.Schemas;
using ChatApp.Tables;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDChatApp.ChatApp.Schemas;

public class MMessageContent : MDataclass<IChatMessage>
{
    [BsonId]
    [BsonElement("message_id")]
    public string MessageId { get; set; }
    
    [BsonElement("sender_id")]
    public string SenderId { get; set; }
    
    [BsonElement("room_id")]
    public string RoomId { get; set; }
    
    [BsonElement("message_content")]
    public string MessageContent { get; set; }
    
    [BsonElement("created_at")]
    public long CreatedAt { get; set; }
    
    [BsonElement("modified_at")]
    public long ModifiedAt { get; set; }
    
    [BsonElement("client_stamp")]
    public long ClientStamp { get; set; }
    
    public override IChatMessage ToCommon()
    {
        return new ChatMessage()
        {
            MessageId = MessageId,
            SenderId = SenderId,
            RoomId = RoomId,
            MessageContent = MessageContent,
            CreatedAt = CreatedAt,
            ModifiedAt = ModifiedAt,
            ClientStamp = ClientStamp,
        };
    }

    public override void FromCommon(IChatMessage common)
    {
        MessageId = common.GetMessageId();
        SenderId = common.GetSenderId();
        RoomId = common.GetRoomId();
        MessageContent = common.GetMessageContent();
        CreatedAt = common.GetCreationTime();
        ModifiedAt = common.GetModificationTime();
        ClientStamp = common.GetClientStamp();
    }
    
    public static MMessageContent Marshall(IChatMessage common)
        => Marshall<MMessageContent>(common);
}