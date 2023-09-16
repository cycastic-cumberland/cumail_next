using ChatApp.Schemas;

namespace ChatApp.Tables;

public class PartialReaction : DictionaryPartialDataclass, IChatReaction
{
    public PartialReaction(Dictionary<string, object> anonymousObject) : base(anonymousObject)
    {
    }

    public string GetUserId() => Fetch<string>("UserId");
    public string GetMessageId() => Fetch<string>("MessageId");
    public string GetEmoji() => Fetch<string>("ReactionEmoji");
}

public class MessageReaction : IChatReaction
{
    public string MessageId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string ReactionEmoji { get; set; } = "";
    public string GetUserId()
    {
        return UserId;
    }

    public string GetMessageId()
    {
        return MessageId;
    }

    public string GetEmoji()
    {
        return ReactionEmoji;
    }
}