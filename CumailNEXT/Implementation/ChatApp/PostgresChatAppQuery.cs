using System.Data;
using CumailNEXT.Components.ChatApp;
using CumailNEXT.Components.ChatApp.Schemas;
using CumailNEXT.Components.Database;
using CumailNEXT.Implementation.ChatApp.Tables;
using CumailNEXT.Implementation.Database;

namespace CumailNEXT.Implementation.ChatApp;

public class PostgresChatAppQueryFactory : ChatAppQueryFactory
{
    private readonly PostgresConnectionSettings _conn;

    public PostgresChatAppQueryFactory(PostgresConnectionSettings conn)
    {
        _conn = conn;
    }

    public PostgresChatAppQuery NewQueryInstance() => new(_conn);
    public override ChatAppQuery CreateInstance() => NewQueryInstance();
}

public class PostgresChatAppQuery : ChatAppQuerySql
{
    private readonly PostgresProvider _provider;
    private readonly bool _autoClose;
    private PostgresTransaction? _currentTransaction;

    private IDbTransaction? OngoingTransaction
    {
        get
        {
            IDbTransaction? re;
            lock (this)
            {
                re = _currentTransaction?.GetRawTransaction();
            }

            return re;
        }
    }
    
    private int CountRows(string tableName)
    {
        var re = ParameterizedQuery<QueryCount>("SELECT COUNT(*) AS RowCount FROM @tName;", new { tName = tableName }).ToList();
        return re.Count == 0 ? 0 : re[0].RowCount;
    }
    private int CountRowsManual(string sql, object? param = null)
    {
        var re = ParameterizedQuery<QueryCount>(sql, param).ToList();
        return re.Count == 0 ? 0 : re[0].RowCount;
    }
    private PostgresTransaction CreateTransactionInternal() => _provider.CreateTransaction();
    protected override ITransaction CreateTransaction() => CreateTransactionInternal();
    private IEnumerable<T> ParameterizedQuery<T>(string sql, object? param = null)
    {
        return _provider.MappedQuery<T>(sql, param, OngoingTransaction);
    }

    private int ExecuteQuery(string query, object? param = null)
    {
        return _provider.Execute(query, param, OngoingTransaction);
    }

    public PostgresChatAppQuery(PostgresConnectionSettings connectionSettings)
    {
        _autoClose = true;
        _provider = new PostgresProvider(connectionSettings);
    }
    
    public PostgresChatAppQuery(PostgresProvider provider)
    {
        _autoClose = false;
        _provider = provider;
    }
    public override void Dispose()
    {
        if (_autoClose) _provider.Dispose();
    }
    public override IChatUserProfile? GetUserById(string userId)
    {
        var re = InquireUsers("SELECT uuid as UserId, " +
                              "username as UserName, " +
                              "pfp_url as ProfilePictureUrl, " +
                              "description as Description, " +
                              "is_disabled as Disabled, " +
                              "is_deleted as Deleted " +
                              "FROM user_profiles WHERE uuid = @uid LIMIT 1", 
            new { uid = userId });
        return re.Count == 0 ? null : re[0];
    }

    public override List<IChatUserProfile> GetUsersByRoomId(string roomId)
    {
        var re = InquireUsers("SELECT user_profiles.uuid as UserId, " +
                              "user_profiles.username as UserName, " +
                              "user_profiles.pfp_url as ProfilePictureUrl, " +
                              "user_profiles.description as Description, " +
                              "user_profiles.is_disabled as Disabled, " +
                              "user_profiles.is_deleted as Deleted " +
                              "FROM chat_rooms_personnel " +
                              "INNER JOIN user_profiles on user_profiles.uuid = chat_rooms_personnel.user_id " +
                              "WHERE chat_rooms_personnel.chat_room_id = @cid;",
            new { cid = roomId });
        return re;
    }
    public override List<IChatRoomPersonnel> GetPersonnelByUserId(string userId)
    {
        var re = InquirePersonnel("SELECT chat_room_id as ChatRoomId, " +
                              "user_id as UserId, " +
                              "role as Role " +
                              "FROM chat_rooms_personnel " +
                              "WHERE user_id = @uid;",
            new { uid = userId});
        return re;
    }

    public override List<IChatRoomPersonnel> GetPersonnelByRoomId(string roomId)
    {
        var re = InquirePersonnel("SELECT chat_room_id as ChatRoomId, " +
                                  "user_id as UserId, " +
                                  "role as Role " +
                                  "FROM chat_rooms_personnel " +
                                  "WHERE chat_room_id = @cid;",
            new { cid = roomId});
        return re;
    }

    public override IChatRoomPersonnel? GetPersonnelByUserAndRoomId(string userId, string roomId)
    {
        var re = InquirePersonnel("SELECT chat_room_id as ChatRoomId, " +
                                  "user_id as UserId, " +
                                  "role as Role " +
                                  "FROM chat_rooms_personnel " +
                                  "WHERE user_id = @uid AND chat_room_id = @cid LIMIT 1;",
            new { uid = userId, cid = roomId});
        return re.Count == 0 ? null : re[0];
    }

    public override IChatRoomProfile? GetChatRoomById(string roomId)
    {
        var re = InquireChatRoom("SELECT room_id as RoomId, " +
                                 "room_name as RoomName, " +
                                 "description as Description, " +
                                 "created_at as CreatedAt, last_activity as LastActivity," +
                                 "is_public as IsPublic," +
                                 "hashed_password as HashedPassword, " +
                                 "max_users as MaxUsers " +
                                 "FROM chat_rooms_info " +
                                 "WHERE room_id = @cid LIMIT 1;",
            new { cid = roomId });
        return re.Count == 0 ? null : re[0];
    }

    public override List<IChatRoomProfile> GetChatRoomByVisibility(bool isPublic)
    {
        var re = InquireChatRoom("SELECT room_id as RoomId, " +
                                 "room_name as RoomName, " +
                                 "description as Description, " +
                                 "created_at as CreatedAt, last_activity as LastActivity," +
                                 "is_public as IsPublic," +
                                 "hashed_password as HashedPassword, " +
                                 "max_users as MaxUsers " +
                                 "FROM chat_rooms_info " +
                                 "WHERE is_public = @visibility;",
            new { visibility = isPublic });
        return re;
    }
    public override IChatRoomInvitation? GetInvitationById(string id)
    {
        var re = InquireInvitations("SELECT invitation_string as InvitationString, " +
                                    "instigator_id as InstigatorId, " +
                                    "chat_room_id as ChatRoomId, " +
                                    "is_enabled as IsEnabled " +
                                    "FROM chat_room_invitations " +
                                    "WHERE invitation_string = @invString LIMIT 1;",
            new { invString = id });
        return re.Count == 0 ? null : re[0];
    }

    public override List<IChatRoomInvitation> GetInvitationsByRoomId(string roomId)
    {
        return InquireInvitations("SELECT invitation_string as InvitationString, " +
                                  "instigator_id as InstigatorId, " +
                                  "chat_room_id as ChatRoomId, " +
                                  "is_enabled as IsEnabled " +
                                  "FROM chat_room_invitations " +
                                  "WHERE chat_room_id = @chatRoomId;",
            new { chatRoomId = roomId });
    }

    public override List<IChatRoomInvitation> GetInvitationsByInstigatorId(string instigatorId)
    {
        return InquireInvitations("SELECT invitation_string as InvitationString, " +
                                  "instigator_id as InstigatorId, " +
                                  "chat_room_id as ChatRoomId, " +
                                  "is_enabled as IsEnabled " +
                                  "FROM chat_room_invitations " +
                                  "WHERE instigator_id = @userId;",
            new { userId = instigatorId });
    }

    public override IChatMessage? GetMessageById(string id)
    {
        var re = InquireMessages("SELECT message_contents.message_id as MessageId, " +
                                 "monolithic_messages_pool.sender_id as SenderId, " +
                                 "monolithic_messages_pool.chat_room_id as RoomId,  " +
                                 "message_contents.message_content as MessageContent, " +
                                 "monolithic_messages_pool.insertion_time as CreatedAt, " +
                                 "monolithic_messages_pool.modification_time as ModifiedAt, " +
                                 "monolithic_messages_pool.client_stamp as ClientStamp " +
                                 "FROM message_contents " +
                                 "INNER JOIN monolithic_messages_pool on monolithic_messages_pool.message_id = message_contents.message_id " +
                                 "WHERE message_contents.message_id = @messageId LIMIT 1;",
            new { messageId = id });
        return re.Count == 0 ? null : re[0];
    }

    public override List<IChatMessage> GetMessagesByRoomId(string roomId)
    {
        return InquireMessages("SELECT message_contents.message_id as MessageId, " +
                        "monolithic_messages_pool.sender_id as SenderId, " +
                        "monolithic_messages_pool.chat_room_id as RoomId,  " +
                        "message_contents.message_content as MessageContent, " +
                        "monolithic_messages_pool.insertion_time as CreatedAt, " +
                        "monolithic_messages_pool.modification_time as ModifiedAt, " +
                        "monolithic_messages_pool.client_stamp as ClientStamp " +
                        "FROM monolithic_messages_pool " +
                        "INNER JOIN message_contents on message_contents.message_id = monolithic_messages_pool.message_id " +
                        "WHERE monolithic_messages_pool.chat_room_id = @chatRoomId;",
            new { chatRoomId = roomId });
    }

    public override List<IChatMessage> GetMessagesBySenderId(string senderId)
    {
        return InquireMessages("SELECT message_contents.message_id as MessageId, " +
                               "monolithic_messages_pool.sender_id as SenderId, " +
                               "monolithic_messages_pool.chat_room_id as RoomId,  " +
                               "message_contents.message_content as MessageContent, " +
                               "monolithic_messages_pool.insertion_time as CreatedAt, " +
                               "monolithic_messages_pool.modification_time as ModifiedAt, " +
                               "monolithic_messages_pool.client_stamp as ClientStamp " +
                               "FROM monolithic_messages_pool " +
                               "INNER JOIN message_contents on message_contents.message_id = monolithic_messages_pool.message_id " +
                               "WHERE monolithic_messages_pool.sender_id = @userId;",
            new { userId = senderId });
    }

    public override List<IChatMessage> GetMessagesByRoomAndSender(string roomId, string senderId)
    {
        return InquireMessages("SELECT message_contents.message_id as MessageId, " +
                               "monolithic_messages_pool.sender_id as SenderId, " +
                               "monolithic_messages_pool.chat_room_id as RoomId,  " +
                               "message_contents.message_content as MessageContent, " +
                               "monolithic_messages_pool.insertion_time as CreatedAt, " +
                               "monolithic_messages_pool.modification_time as ModifiedAt, " +
                               "monolithic_messages_pool.client_stamp as ClientStamp " +
                               "FROM monolithic_messages_pool " +
                               "INNER JOIN message_contents on message_contents.message_id = monolithic_messages_pool.message_id " +
                               "WHERE monolithic_messages_pool.chat_room_id = @chatRoomId AND monolithic_messages_pool.sender_id = @userId;",
            new { chatRoomId = roomId, userId = senderId });
    }

    public override List<IChatMessage> GetMessageBySlice(string roomId, int count, Order sortOrder)
    {
        if (sortOrder == Order.Ascending)
        {
            return InquireMessages("SELECT message_contents.message_id as MessageId, " +
                                   "monolithic_messages_pool.sender_id as SenderId, " +
                                   "monolithic_messages_pool.chat_room_id as RoomId,  " +
                                   "message_contents.message_content as MessageContent, " +
                                   "monolithic_messages_pool.insertion_time as CreatedAt, " +
                                   "monolithic_messages_pool.modification_time as ModifiedAt, " +
                                   "monolithic_messages_pool.client_stamp as ClientStamp " +
                                   "FROM monolithic_messages_pool " +
                                   "INNER JOIN message_contents on message_contents.message_id = monolithic_messages_pool.message_id " +
                                   "WHERE monolithic_messages_pool.chat_room_id = @chatRoomId ORDER BY monolithic_messages_pool.insertion_time LIMIT @lim;",
                new { chatRoomId = roomId, lim = count });
        }
        return InquireMessages("SELECT message_contents.message_id as MessageId, " +
                               "monolithic_messages_pool.sender_id as SenderId, " +
                               "monolithic_messages_pool.chat_room_id as RoomId,  " +
                               "message_contents.message_content as MessageContent, " +
                               "monolithic_messages_pool.insertion_time as CreatedAt, " +
                               "monolithic_messages_pool.modification_time as ModifiedAt, " +
                               "monolithic_messages_pool.client_stamp as ClientStamp " +
                               "FROM monolithic_messages_pool " +
                               "INNER JOIN message_contents on message_contents.message_id = monolithic_messages_pool.message_id " +
                               "WHERE monolithic_messages_pool.chat_room_id = @chatRoomId ORDER BY monolithic_messages_pool.insertion_time DESC LIMIT @lim;",
            new { chatRoomId = roomId, lim = count });
    }
    
    public override IChatReaction? GetReactionByKeyPair(string userId, string messageId)
    {
        var re = InquireReactions("SELECT message_id as MessageId, " +
                                  "user_id as UserId, " +
                                  "reaction_emoji as ReactionEmoji " +
                                  "FROM message_reactions " +
                                  "WHERE user_id = @uid AND message_id = @mid LIMIT 1;",
            new { uid = userId, mid = messageId });
        return re.Count == 0 ? null : re[0];
    }

    public override List<IChatReaction> GetReactionsByMessageId(string messageId)
    {
        return InquireReactions("SELECT message_id as MessageId, " +
                                "user_id as UserId, " +
                                "reaction_emoji as ReactionEmoji " +
                                "FROM message_reactions " +
                                "WHERE message_id = @mid;",
            new { mid = messageId });
    }
    public override void AddUser(IChatUserProfile user)
    {
        ExecuteQuery("INSERT INTO user_profiles (uuid, username, pfp_url, description, is_disabled, is_deleted) VALUES (@uid, @username, @pfp, @des, @disabled, @deleted);", 
            new { uid = user.GetUserId(), username = user.GetUserName(), pfp = user.GetPfpUrl(), des = user.GetDescription(), disabled = user.IsDisabled(), deleted = user.IsDeleted() });
    }
    public override void AddPersonnel(IChatRoomPersonnel personnel)
    {
        ExecuteQuery(
            "INSERT INTO chat_rooms_personnel (user_id, chat_room_id, role) VALUES (@uid, @cid, @role);",
            new { uid = personnel.GetUserId(), cid = personnel.GetRoomId(), role = personnel.GetRole() });
    }
    public override void AddRoom(IChatRoomProfile chatRoom)
    {
        ExecuteQuery("INSERT INTO chat_rooms_info (room_id, room_name, description, created_at, last_activity, is_public, hashed_password, max_users) VALUES (@cid, @cName, @des, @it, @la, @visible, @pass, @max);",
            new { cid = chatRoom.GetRoomId(), cName = chatRoom.GetRoomName(), des = chatRoom.GetDescription(), it = chatRoom.GetCreationTime(), la = chatRoom.GetLastActivity(), visible = chatRoom.IsVisible(), pass = chatRoom.GetHashedPassword(), max = chatRoom.GetMaxUsers() });
    }
    public override void AddInvitation(IChatRoomInvitation invitation)
    {
        ExecuteQuery(
            "INSERT INTO chat_room_invitations (invitation_string, instigator_id, chat_room_id, is_enabled) VALUES (@invString, @insString, @cid, @enabled);",
            new { invString = invitation.GetInvitationId(), insString = invitation.GetInstigatorId(), cid = invitation.GetRoomId(), enabled = invitation.IsEnabled() });
    }
    public override void AddMessage(IChatMessage message)
    {
        OpenTransaction((_, _) =>
        {
            ExecuteQuery(
                "INSERT INTO message_contents (message_id, message_content) VALUES (@mid, @mc);",
                new { mid = message.GetMessageId(), mc = message.GetMessageContent() });
            ExecuteQuery(
                "INSERT INTO monolithic_messages_pool (message_id, chat_room_id, sender_id, insertion_time, modification_time, client_stamp) VALUES (@mid, @cid, @sid, @it, @mt, @cs);",
                new { mid = message.GetMessageId(), cid = message.GetRoomId(), sid = message.GetSenderId(), it = message.GetCreationTime(), mt = message.GetModificationTime(), cs = message.GetClientStamp() });
        });
    }

    public override void AddReaction(IChatReaction reaction)
    {
        ExecuteQuery(
            "INSERT INTO message_reactions (message_id, user_id, reaction_emoji) VALUES (@mid, @uid, @emo);",
            new { mid = reaction.GetMessageId(), uid = reaction.GetUserId(), emo = reaction.GetEmoji() });
    }

    public override void UpdateUser(IChatUserProfile user)
    {
        ExecuteQuery("UPDATE user_profiles SET username = @username, pfp_url = @pfp, description = @des, is_disabled = @dis, is_deleted = @del WHERE uuid = @uid;",
            new { username = user.GetUserName(), pfp = user.GetPfpUrl(), des = user.GetDescription(), dis = user.IsDisabled(), del = user.IsDeleted(), uid = user.GetUserId() });
    }

    public override void UpdatePersonnel(IChatRoomPersonnel personnel)
    {
        ExecuteQuery("UPDATE chat_rooms_personnel SET role = @role WHERE user_id = @uid AND chat_room_id = @cid;",
            new { role = personnel.GetRole(), uid = personnel.GetUserId(), cid = personnel.GetRoomId() });
    }

    public override void UpdateRoom(IChatRoomProfile chatRoom)
    {
        ExecuteQuery("UPDATE chat_rooms_info SET room_name = @cName, description = @des, created_at = @it, last_activity = @la, is_public = @visible, hashed_password = @pass, max_users = @max WHERE room_id = @cid;",
            new { cName = chatRoom.GetRoomName(), des = chatRoom.GetDescription(), it = chatRoom.GetCreationTime(), la = chatRoom.GetLastActivity(), visible = chatRoom.IsVisible(), pass = chatRoom.GetHashedPassword(), max = chatRoom.GetMaxUsers(), cid = chatRoom.GetRoomId() });
    }

    public override void UpdateInvitation(IChatRoomInvitation invitation)
    {
        ExecuteQuery("UPDATE chat_room_invitations SET is_enabled = @enabled, chat_room_id = @cid, instigator_id = @uid WHERE invitation_string = @inv;",
            new { enabled = invitation.IsEnabled(), cid = invitation.GetRoomId(), uid = invitation.GetInstigatorId(), inv = invitation.GetInvitationId() });
    }

    public override void UpdateMessage(IChatMessage message)
    {
        OpenTransaction((_, _) =>
        {
            ExecuteQuery("UPDATE message_contents SET message_content = @content WHERE message_id = @mid;",
                new { content = message.GetMessageContent(), mid = message.GetMessageId() });
            ExecuteQuery("UPDATE monolithic_messages_pool SET sender_id = @uid, insertion_time = @it, modification_time = @mt AND client_stamp = @cs WHERE message_id = @mid AND chat_room_id = @cid;",
                new { uid = message.GetSenderId(), it = message.GetCreationTime(), mt = message.GetModificationTime(), cs = message.GetClientStamp(), mid = message.GetMessageId(), cid = message.GetRoomId() });
        });
    }

    public override void UpdateReaction(IChatReaction reaction)
    {
        ExecuteQuery("UPDATE message_reactions SET reaction_emoji = @emoji WHERE message_id = @mid AND user_id = @uid;",
            new { emoji = reaction.GetEmoji(), mid = reaction.GetMessageId(), uid = reaction.GetUserId() });
    }

    public override int RemoveUser(string userId)
    {
        return ExecuteQuery("DELETE FROM user_profiles WHERE uuid = @uid;", new { uid = userId });
    }

    public override int RemovePersonnel(string userId, string roomId)
    {
        return ExecuteQuery("DELETE FROM chat_rooms_personnel WHERE user_id = @uid AND chat_room_id = @cid;", 
            new { uid = userId, cid = roomId });
    }

    public override int RemoveRoom(string roomId)
    {
        return ExecuteQuery("DELETE FROM chat_rooms_info WHERE room_id = @cid;",
            new { cid = roomId });
    }

    public override int RemoveInvitation(string invitationString)
    {
        return ExecuteQuery("DELETE FROM chat_room_invitations WHERE invitation_string = @inv;", 
            new { inv = invitationString });
    }

    public override int RemoveMessage(string messageId)
    {
        return OpenTransaction((_, _) =>
        {
            var total = 0;
            total += ExecuteQuery("DELETE FROM monolithic_messages_pool WHERE message_id = @mid;",
                new { mid = messageId });
            total += ExecuteQuery("DELETE FROM message_contents WHERE message_id = @mid;",
                new { mid = messageId });
            return total;
        });
    }

    public override int RemoveReaction(string userId, string messageId)
    {
        return ExecuteQuery("DELETE FROM message_reactions WHERE user_id = @uid AND message_id = @mid;",
            new { uid = userId, mid = messageId });
    }

    public override void EraseRoom(string roomId)
    {
        OpenTransaction((_, _) =>
        {
            ExecuteQuery("DELETE FROM monolithic_messages_pool WHERE chat_room_id = @rid;" +
                         "DELETE FROM chat_rooms_personnel WHERE chat_room_id = @rid;" +
                         "DELETE FROM message_contents WHERE message_id IN " +
                         "(SELECT message_id FROM monolithic_messages_pool WHERE chat_room_id = @rid);" +
                         "DELETE FROM message_reactions WHERE message_id IN " +
                         "(SELECT message_id FROM monolithic_messages_pool WHERE chat_room_id = @rid);" +
                         "DELETE FROM chat_room_invitations WHERE chat_room_id = @rid;" +
                         "DELETE FROM chat_rooms_info WHERE room_id = @rid;", new { rid = roomId });
        });
    }

    public override void UpdateRoomActivity(string roomId)
    {
        ExecuteQuery("UPDATE chat_rooms_info SET last_activity = @la WHERE room_id = @cid;",
            new { la = DateTimeOffset.Now.ToUnixTimeSeconds(), cid = roomId });
    }

    public override void OpenTransaction(Action<ChatAppQuery, ITransaction> action)
    {
        lock (this)
        {
            // Only one transaction can be process per thread per connection with Npgsql
            PostgresTransaction transaction;
            bool isTopLevel;
            // No running transaction
            if (_currentTransaction == null)
            {
                transaction = CreateTransactionInternal();
                isTopLevel = true;
            }
            else
            {
                transaction = _currentTransaction;
                isTopLevel = false;
            }
        
            try
            {
                if (isTopLevel) transaction.Start();
                action(this, transaction);
                if (!isTopLevel) return;
                transaction.Commit();
                transaction.Dispose();
                _currentTransaction = null;
            }
            catch (Exception)
            {
                // If there's an exception, rollback top level transaction, and dispose
                _currentTransaction?.RollBack();
                _currentTransaction?.Dispose();
                _currentTransaction = null;
                throw;
            }
        }
    }
    public override T OpenTransaction<T>(Func<ChatAppQuery, ITransaction, T> action)
    {
        lock (this)
        {
            // Only one transaction can be process per thread per connection with Npgsql
            PostgresTransaction transaction;
            bool isTopLevel;
            // No running transaction
            if (_currentTransaction == null)
            {
                transaction = CreateTransactionInternal();
                isTopLevel = true;
            }
            else
            {
                transaction = _currentTransaction;
                isTopLevel = false;
            }
        
            try
            {
                if (isTopLevel) transaction.Start();
                var re = action(this, transaction);
                if (!isTopLevel) return re;
                transaction.Commit();
                transaction.Dispose();
                _currentTransaction = null;
                return re;
            }
            catch (Exception)
            {
                // If there's an exception, rollback top level transaction, and dispose
                _currentTransaction?.RollBack();
                _currentTransaction?.Dispose();
                _currentTransaction = null;
                throw;
            }
        }
    }

    public override List<IChatUserProfile> InquireUsers(string sql, object? param = null) =>
        ParameterizedQuery<UserProfile>(sql, param).Select(v => (IChatUserProfile)v).ToList();
    public override List<IChatRoomPersonnel> InquirePersonnel(string sql, object? param = null) =>
        ParameterizedQuery<ChatRoomPersonnel>(sql, param).Select(v => (IChatRoomPersonnel)v).ToList();

    public override List<IChatRoomProfile> InquireChatRoom(string sql, object? param = null) =>
        ParameterizedQuery<ChatRoomInfo>(sql, param).Select(v => (IChatRoomProfile)v).ToList();
    public override List<IChatRoomInvitation> InquireInvitations(string sql, object? param = null) =>
        ParameterizedQuery<ChatRoomInvitation>(sql, param).Select(v => (IChatRoomInvitation)v).ToList();

    public override List<IChatMessage> InquireMessages(string sql, object? param = null) =>
        ParameterizedQuery<ChatMessage>(sql, param).Select(v => (IChatMessage)v).ToList();

    public override List<IChatReaction> InquireReactions(string sql, object? param = null) =>
        ParameterizedQuery<MessageReaction>(sql, param).Select(v => (IChatReaction)v).ToList();

    public override int TotalUsers() => CountRows("user_profiles");
    public override int TotalPersonnel() => CountRows("chat_rooms_personnel");
    public override int TotalInvitations() => CountRows("chat_room_invitations");
    public override int TotalMessages() => CountRows("message_contents");
    public override int TotalReactions() => CountRows("message_reactions");

    public override int CountReactions(string messageId) => CountRowsManual(
        "SELECT COUNT(*) AS RowCount FROM message_reactions WHERE message_id = @mid;", new { mid = messageId });
}