using CumailNEXT.Components.ChatApp;
using CumailNEXT.Components.ChatApp.Schemas;

namespace CumailNEXT.Implementation.ChatApp;

public abstract class ChatAppQuerySql : ChatAppQuery
{
    public abstract List<IChatUserProfile> InquireUsers(string sql, object? param = null);
    public abstract List<IChatRoomPersonnel> InquirePersonnel(string sql, object? param = null);
    public abstract List<IChatRoomProfile> InquireChatRoom(string sql, object? param = null);
    public abstract List<IChatRoomInvitation> InquireInvitations(string sql, object? param = null);
    public abstract List<IChatMessage> InquireMessages(string sql, object? param = null);
    public abstract List<IChatReaction> InquireReactions(string sql, object? param = null);
}