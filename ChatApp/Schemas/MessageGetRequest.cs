namespace ChatApp.Schemas;

public class MessageGetRequest
{
    public string RoomId { get; set; } = "";
    public string StartId { get; set; } = "";
    public int End { get; set; } = 0;
    public bool IsAscending { get; set; } = true;
}