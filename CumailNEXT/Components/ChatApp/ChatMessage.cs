using System.Reflection;
using CumailNEXT.Components.ChatApp.Schemas;
using CumailNEXT.Components.Core;

namespace CumailNEXT.Components.ChatApp;

public class PartialMessage : DictionaryPartialDataclass, IChatMessage
{

    public PartialMessage(Dictionary<string, object> anonymousObject) : base(anonymousObject)
    {
    }

    public string GetMessageId() => Fetch<string>("MessageId");

    public string GetSenderId() => Fetch<string>("SenderId");

    public string GetRoomId() => Fetch<string>("RoomId");

    public string GetMessageContent() => Fetch<string>("MessageContent");

    public long GetCreationTime() => FetchNative<long>("CreatedAt");

    public long GetModificationTime() => FetchNative<long>("ModifiedAt");
    public long GetClientStamp() => FetchNative<long>("ClientStamp");
}

public class ChatMessage : IChatMessage
{
    public string MessageId { get; set; }
    public string SenderId { get; set; }
    public string RoomId { get; set; }
    public string MessageContent { get; set; }
    public long CreatedAt { get; set; }
    public long ModifiedAt { get; set; }
    public long ClientStamp { get; set; }

    public ChatMessage(string senderId, string roomId, string messageContent, long clientStamp)
    {
        CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        ModifiedAt = CreatedAt;
        SenderId = senderId;
        RoomId = roomId;
        MessageContent = messageContent;
        MessageId = Crypto.HashSha256String($"{SenderId}${CreatedAt}${Crypto.GenerateSecureString(32)}${RoomId}");
        ClientStamp = clientStamp;
    }

    public ChatMessage() : this("", "", "", 0)
    {
        
    }

    public void RegenerateId()
    {
        MessageId = Crypto.HashSha256String($"{SenderId}${CreatedAt}${Crypto.GenerateSecureString(32)}${RoomId}");
    }

    public void ModifyMessage(string newContent)
    {
        MessageContent = newContent;
        ModifiedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    public ChatMessage Duplicate()
    {
        return new ChatMessage
        {
            MessageId = MessageId,
            SenderId = SenderId,
            MessageContent = MessageContent,
            CreatedAt = CreatedAt,
            ModifiedAt = ModifiedAt,
        };
    }

    public string GetMessageId()
    {
        return MessageId;
    }

    public string GetSenderId()
    {
        return SenderId;
    }

    public string GetRoomId()
    {
        return RoomId;
    }

    public string GetMessageContent()
    {
        return MessageContent;
    }

    public long GetCreationTime()
    {
        return CreatedAt;
    }

    public long GetModificationTime()
    {
        return ModifiedAt;
    }

    public long GetClientStamp() => ClientStamp;
}