namespace ChatApp.Schemas;

public interface IChatMessage
{
    public string GetMessageId();
    public string GetSenderId();
    public string GetRoomId();
    public string GetMessageContent();
    public long GetCreationTime();
    public long GetModificationTime();
    public long GetClientStamp();
}