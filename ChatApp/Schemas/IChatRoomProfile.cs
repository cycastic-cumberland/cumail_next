namespace ChatApp.Schemas;

public interface IChatRoomProfile
{
    public string GetRoomId();
    public string GetRoomName();
    public string GetDescription();
    public long GetCreationTime();
    public long GetLastActivity();
    public bool IsVisible();
    public string GetHashedPassword();
    public int GetMaxUsers();
    // Sensitive contents
    public void SetHashedPassword(string newPassword)
    {
        
    }
}