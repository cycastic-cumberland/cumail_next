namespace CumailNEXT.Components.ChatApp.Schemas;

public class MessagePostRequest
{
    public string SenderId { get; set; } = "";
    // public string ChatRoomId { get; set; } = "";
    public string MessageContent { get; set; } = "";
    public long ClientStamp { get; set; }
}