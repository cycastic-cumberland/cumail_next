using CumailNEXT.Components.ChatApp.Schemas;
using CumailNEXT.Components.Database;

namespace CumailNEXT.Components.ChatApp;

public class ChatAppUsersQuery
{
    private readonly ChatAppQuery _query;
    public ChatAppUsersQuery(ChatAppQuery query)
    {
        _query = query;
    }

    public List<IChatUserProfile> ByJoinedRoom(string roomId) => _query.GetUsersByRoomId(roomId);
    public IChatUserProfile? this[string userId]
    {
        get => _query.GetUserById(userId);
        set
        {
            if (value == null)
            {
                _query.RemoveUser(userId);
                return;
            }
            _query.OpenTransaction((_, _) =>
            {
                if (this[userId] == null)
                {
                    _query.AddUser(value);
                }
                else
                {
                    _query.UpdateUser(value);
                }
            });
        }
    }
}

public class ChatAppPersonnelQuery
{
    private readonly ChatAppQuery _query;
    public ChatAppPersonnelQuery(ChatAppQuery query)
    {
        _query = query;
    }

    public List<IChatRoomPersonnel> ByRoom(string roomId) => _query.GetPersonnelByRoomId(roomId);
    public List<IChatRoomPersonnel> ByUser(string userId) => _query.GetPersonnelByUserId(userId);
    public IChatRoomPersonnel? this[string userId, string roomId]
    {
        get => _query.GetPersonnelByUserAndRoomId(userId, roomId);
        set
        {
            if (value == null)
            {
                _query.RemovePersonnel(userId, roomId);
                return;
            }
            _query.OpenTransaction((_, _) =>
            {
                if (this[userId, roomId] == null)
                {
                    _query.AddPersonnel(value);
                }
                else
                {
                    _query.UpdatePersonnel(value);
                }
            });
        }
    }
}

public class ChatAppRoomsQuery
{
    private readonly ChatAppQuery _query;
    public ChatAppRoomsQuery(ChatAppQuery query)
    {
        _query = query;
    }
    public IChatRoomProfile? this[string roomId]
    {
        get => _query.GetChatRoomById(roomId);
        set
        {
            if (value == null)
            {
                _query.RemoveRoom(roomId);
                return;
            }
            _query.OpenTransaction((_, _) =>
            {
                if (this[roomId] == null)
                {
                    _query.AddRoom(value);
                }
                else
                {
                    _query.UpdateRoom(value);
                }
            });
        }
    }
}

public class ChatAppInvitationsQuery
{
    private readonly ChatAppQuery _query;
    public ChatAppInvitationsQuery(ChatAppQuery query)
    {
        _query = query;
    }

    public List<IChatRoomInvitation> ByRoom(string roomId) => _query.GetInvitationsByRoomId(roomId);
    public List<IChatRoomInvitation> ByInstigator(string userId) => _query.GetInvitationsByInstigatorId(userId);
    public IChatRoomInvitation? this[string invitation]
    {
        get => _query.GetInvitationById(invitation);
        set
        {
            if (value == null)
            {
                _query.RemoveInvitation(invitation);
                return;
            }
            _query.OpenTransaction((_, _) =>
            {
                if (this[invitation] == null)
                {
                    _query.AddInvitation(value);
                }
                else
                {
                    _query.UpdateInvitation(value);
                }
            });
        }
    }
}

public class ChatAppMessagesQuery
{
    private readonly ChatAppQuery _query;
    public ChatAppMessagesQuery(ChatAppQuery query)
    {
        _query = query;
    }

    public List<IChatMessage> BySender(string senderId) => _query.GetMessagesBySenderId(senderId);
    public List<IChatMessage> ByRoom(string roomId) => _query.GetMessagesByRoomId(roomId);
    public IChatMessage? this[string messageId]
    {
        get => _query.GetMessageById(messageId);
        set
        {
            if (value == null)
            {
                _query.RemoveMessage(messageId);
                return;
            }
            if (this[messageId] == null)
            {
                _query.AddMessage(value);
            }
            else
            {
                _query.UpdateMessage(value);
            }
        }
    }

    public List<IChatMessage> this[string roomId, string senderId] =>
        _query.GetMessagesByRoomAndSender(roomId, senderId);
}

public class ChatAppReactionsQuery
{
    private readonly ChatAppQuery _query;
    public ChatAppReactionsQuery(ChatAppQuery query)
    {
        _query = query;
    }
    public IChatReaction? this[string senderId, string messageId]
    {
        get => _query.GetReactionByKeyPair(senderId, messageId);
        set
        {
            if (value == null)
            {
                _query.RemoveReaction(senderId, messageId);
                return;
            }
            _query.OpenTransaction((_, _) =>
            {
                if (this[senderId, messageId] == null)
                {
                    _query.AddReaction(value);
                }
                else
                {
                    _query.UpdateReaction(value);
                }
            });
        }
    }
}

public abstract class ChatAppQueryFactory
{
    public abstract ChatAppQuery CreateInstance();
}

public abstract class ChatAppQuery : IDisposable
{
    public enum Order
    {
        Ascending,
        Descending
    }
    public readonly ChatAppUsersQuery Users;
    public readonly ChatAppPersonnelQuery Personnel;
    public readonly ChatAppRoomsQuery Rooms;
    public readonly ChatAppInvitationsQuery Invitations;
    public readonly ChatAppMessagesQuery Messages;
    public readonly ChatAppReactionsQuery Reactions;

    protected ChatAppQuery()
    {
        Users = new ChatAppUsersQuery(this);
        Personnel = new ChatAppPersonnelQuery(this);
        Rooms = new ChatAppRoomsQuery(this);
        Invitations = new ChatAppInvitationsQuery(this);
        Messages = new ChatAppMessagesQuery(this);
        Reactions = new ChatAppReactionsQuery(this);
    }
        
        
    public abstract void Dispose();

    public abstract IChatUserProfile? GetUserById(string userId);
    public abstract List<IChatUserProfile> GetUsersByRoomId(string roomId);
    public abstract List<IChatRoomPersonnel> GetPersonnelByUserId(string userId);
    public abstract List<IChatRoomPersonnel> GetPersonnelByRoomId(string roomId);
    public abstract IChatRoomPersonnel? GetPersonnelByUserAndRoomId(string userId, string roomId);
    public abstract IChatRoomProfile? GetChatRoomById(string roomId);
    public abstract List<IChatRoomProfile> GetChatRoomByVisibility(bool isPublic);
    public abstract IChatRoomInvitation? GetInvitationById(string id);
    public abstract List<IChatRoomInvitation> GetInvitationsByRoomId(string roomId);
    public abstract List<IChatRoomInvitation> GetInvitationsByInstigatorId(string instigatorId);
    public abstract IChatMessage? GetMessageById(string id);
    public abstract List<IChatMessage> GetMessagesByRoomId(string roomId);
    public abstract List<IChatMessage> GetMessagesBySenderId(string senderId);
    public abstract List<IChatMessage> GetMessagesByRoomAndSender(string roomId, string senderId);
    public abstract List<IChatMessage> GetMessageBySlice(string roomId, int count, Order sortOrder);
    public abstract IChatReaction? GetReactionByKeyPair(string userId, string messageId);
    public abstract List<IChatReaction> GetReactionsByMessageId(string messageId);

    public abstract int TotalUsers();
    public abstract int TotalPersonnel();
    public abstract int TotalInvitations();
    public abstract int TotalMessages();
    public abstract int TotalReactions();

    public abstract int CountReactions(string messageId);

    public abstract void AddUser(IChatUserProfile user);
    public abstract void AddPersonnel(IChatRoomPersonnel personnel);
    public abstract void AddRoom(IChatRoomProfile chatRoom);
    public abstract void AddInvitation(IChatRoomInvitation invitation);
    public abstract void AddMessage(IChatMessage message);
    public abstract void AddReaction(IChatReaction reaction);

    public abstract void UpdateUser(IChatUserProfile user);
    public abstract void UpdatePersonnel(IChatRoomPersonnel personnel);
    public abstract void UpdateRoom(IChatRoomProfile chatRoom);
    public abstract void UpdateInvitation(IChatRoomInvitation invitation);
    public abstract void UpdateMessage(IChatMessage message);
    public abstract void UpdateReaction(IChatReaction reaction);

    public abstract int RemoveUser(string userId);
    public abstract int RemovePersonnel(string userId, string roomId);
    public abstract int RemoveRoom(string roomId);
    public abstract int RemoveInvitation(string invitationString);
    public abstract int RemoveMessage(string messageId);
    public abstract int RemoveReaction(string userId, string messageId);
    public abstract void EraseRoom(string roomId);

    public abstract void UpdateRoomActivity(string roomId);
    protected abstract ITransaction CreateTransaction();
    public virtual void OpenTransaction(Action<ChatAppQuery, ITransaction> action)
    {
        var transaction = CreateTransaction();
        try
        {
            transaction.Start();
            action(this, transaction);
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.RollBack();
            throw;
        }
    }
    public virtual T OpenTransaction<T>(Func<ChatAppQuery, ITransaction, T> action)
    {
        var transaction = CreateTransaction();
        try
        {
            transaction.Start();
            var re = action(this, transaction);
            transaction.Commit();
            return re;
        }
        catch (Exception)
        {
            transaction.RollBack();
            throw;
        }
    }
}