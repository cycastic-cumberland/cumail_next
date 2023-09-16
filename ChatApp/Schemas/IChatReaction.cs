namespace ChatApp.Schemas;

public interface IChatReaction
{
    public string GetUserId();
    public string GetMessageId();
    public string GetEmoji();
}