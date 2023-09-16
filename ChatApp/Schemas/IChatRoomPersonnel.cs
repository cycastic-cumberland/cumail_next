namespace ChatApp.Schemas;

public interface IChatRoomPersonnel
{
    public string GetUserId();
    public string GetRoomId();
    public int GetRole();
}