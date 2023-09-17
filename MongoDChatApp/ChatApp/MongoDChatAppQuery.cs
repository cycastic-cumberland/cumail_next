using ChatApp;
using ChatApp.Schemas;
using CoreComponents.Database;
using MongoDB.Driver;
using MongoDChatApp.ChatApp.Schemas;
using MongoDChatApp.Database;

namespace MongoDChatApp.ChatApp;

public class MongoDChatAppQueryFactory : ChatAppQueryFactory
{
    private readonly MongoDConnectionSettings _conn;
    private readonly Queue<MongoDChatAppQuery> _objectQueue = new();

    public MongoDChatAppQueryFactory(MongoDConnectionSettings conn) => _conn = conn;

    public MongoDChatAppQuery NewQueryInstance()
    {
        MongoDChatAppQuery ret;
        lock (this)
        {
            ret = _objectQueue.Count == 0 ? new(this, _conn) : _objectQueue.Dequeue();
        }
        return ret;
    }

    public void ReturnQuery(MongoDChatAppQuery ret)
    {
        lock (this)
        {
            _objectQueue.Enqueue(ret);
        }
    }

    public override ChatAppQuery CreateInstance() => NewQueryInstance();
}

public class MongoDChatAppQuery : ChatAppQuery
{
    private readonly MongoDProvider _provider;
    private readonly MongoDChatAppQueryFactory _host;
    private MongoDTransaction? _currentTransaction;

    public MongoDChatAppQuery(MongoDChatAppQueryFactory host, MongoDConnectionSettings conn)
    {
        _host = host;
        _provider = new(conn);
    }

    public override void Dispose()
    {
        _host.ReturnQuery(this);
    }
    public override IChatUserProfile? GetUserById(string userId)
    {
        var collection = _provider.GetCollection<MUserProfile>("user_profiles");
        var query = (from doc in collection.AsQueryable()
            where doc.UserId == userId
            select doc);
        foreach (var doc in query)
        {
            return doc.ToCommon();
        }

        return null;
    }

    public override List<IChatUserProfile> GetUsersByRoomId(string roomId)
    {
        var userCollection = _provider.GetCollection<MUserProfile>("user_profiles");
        var personnelCollection = _provider.GetCollection<MChatRoomPersonnel>("chat_rooms_personnel");
        var query = (from personnel in personnelCollection.AsQueryable()
            join user in userCollection.AsQueryable() on personnel.UserId equals user.UserId
            where personnel.ChatRoomId == roomId
            select user);
        return query.ToList().Select(v => v.ToCommon()).ToList();
    }

    public override List<IChatRoomPersonnel> GetPersonnelByUserId(string userId)
    {
        var collection = _provider.GetCollection<MChatRoomPersonnel>("chat_rooms_personnel");
        var query = (from doc in collection.AsQueryable()
            where doc.UserId == userId
            select doc);
        return query.ToList().Select(v => v.ToCommon()).ToList();
    }

    public override List<IChatRoomPersonnel> GetPersonnelByRoomId(string roomId)
    {
        var collection = _provider.GetCollection<MChatRoomPersonnel>("chat_rooms_personnel");
        var query = (from doc in collection.AsQueryable()
            where doc.ChatRoomId == roomId
            select doc);
        return query.ToList().Select(v => v.ToCommon()).ToList();
    }

    public override IChatRoomPersonnel? GetPersonnelByUserAndRoomId(string userId, string roomId)
    {
        var collection = _provider.GetCollection<MChatRoomPersonnel>("chat_rooms_personnel");
        var query = (from doc in collection.AsQueryable()
            where doc.ChatRoomId == roomId && doc.UserId == userId
            select doc);
        foreach (var doc in query)
        {
            return doc.ToCommon();
        }

        return null;
    }

    public override IChatRoomProfile? GetChatRoomById(string roomId)
    {
        var collection = _provider.GetCollection<MChatRoomInfo>("chat_rooms_info");
        var query = (from doc in collection.AsQueryable()
            where doc.RoomId == roomId
            select doc);
        foreach (var doc in query)
        {
            return doc.ToCommon();
        }

        return null;
    }

    public override List<IChatRoomProfile> GetChatRoomByVisibility(bool isPublic)
    {
        var collection = _provider.GetCollection<MChatRoomInfo>("chat_rooms_info");
        var query = (from doc in collection.AsQueryable()
            where doc.IsPublic == isPublic
            select doc);
        return query.ToList().Select(v => v.ToCommon()).ToList();
    }

    public override IChatRoomInvitation? GetInvitationById(string id)
    {
        var collection = _provider.GetCollection<MChatRoomInvitation>("chat_room_invitations");
        var query = (from doc in collection.AsQueryable()
            where doc.InvitationString == id
            select doc);
        foreach (var doc in query)
        {
            return doc.ToCommon();
        }

        return null;
    }

    public override List<IChatRoomInvitation> GetInvitationsByRoomId(string roomId)
    {
        var collection = _provider.GetCollection<MChatRoomInvitation>("chat_room_invitations");
        var query = (from doc in collection.AsQueryable()
            where doc.ChatRoomId == roomId
            select doc);
        return query.ToList().Select(v => v.ToCommon()).ToList();
    }

    public override List<IChatRoomInvitation> GetInvitationsByInstigatorId(string instigatorId)
    {
        var collection = _provider.GetCollection<MChatRoomInvitation>("chat_room_invitations");
        var query = (from doc in collection.AsQueryable()
            where doc.InstigatorId == instigatorId
            select doc);
        return query.ToList().Select(v => v.ToCommon()).ToList();
    }

    public override IChatMessage? GetMessageById(string id)
    {
        var collection = _provider.GetCollection<MMessageContent>("message_contents");
        var query = (from doc in collection.AsQueryable()
            where doc.MessageId == id
            select doc);
        foreach (var doc in query)
        {
            return doc.ToCommon();
        }

        return null;
    }

    public override List<IChatMessage> GetMessagesByRoomId(string roomId)
    {
        var collection = _provider.GetCollection<MMessageContent>("message_contents");
        var query = (from doc in collection.AsQueryable()
            where doc.RoomId == roomId
            select doc);
        return query.ToList().Select(v => v.ToCommon()).ToList();
    }

    public override List<IChatMessage> GetMessagesBySenderId(string senderId)
    {
        var collection = _provider.GetCollection<MMessageContent>("message_contents");
        var query = (from doc in collection.AsQueryable()
            where doc.SenderId == senderId
            select doc);
        return query.ToList().Select(v => v.ToCommon()).ToList();
    }

    public override List<IChatMessage> GetMessagesByRoomAndSender(string roomId, string senderId)
    {
        var collection = _provider.GetCollection<MMessageContent>("message_contents");
        var query = (from doc in collection.AsQueryable()
            where doc.SenderId == senderId && doc.RoomId == roomId
            select doc);
        return query.ToList().Select(v => v.ToCommon()).ToList();
    }

    public override List<IChatMessage> GetMessageBySlice(string roomId, int count, Order sortOrder)
    {
        var collection = _provider.GetCollection<MMessageContent>("message_contents");
        IQueryable<MMessageContent> query;
        if (sortOrder == Order.Ascending)
            query = (from doc in collection.AsQueryable()
                where doc.RoomId == roomId
                orderby doc.CreatedAt ascending
                select doc);
        else
            query = (from doc in collection.AsQueryable()
                where doc.RoomId == roomId
                orderby doc.CreatedAt descending
                select doc);
        return query.Take(count).ToList().Select(v => v.ToCommon()).ToList();
    }

    public override IChatReaction? GetReactionByKeyPair(string userId, string messageId)
    {
        throw new NotImplementedException();
    }

    public override List<IChatReaction> GetReactionsByMessageId(string messageId)
    {
        throw new NotImplementedException();
    }

    private int CountDocuments<T>(string collectionName)
    {
        var collection = _provider.GetCollection<T>(collectionName);
        return (int)collection.CountDocuments(FilterDefinition<T>.Empty);
    }

    public override int TotalUsers() => CountDocuments<MUserProfile>("user_profiles");

    public override int TotalPersonnel() => CountDocuments<MChatRoomPersonnel>("chat_rooms_personnel");

    public override int TotalInvitations() => CountDocuments<MChatRoomInvitation>("chat_room_invitations");

    public override int TotalMessages() => CountDocuments<MMessageContent>("message_contents");

    public override int TotalReactions()
    {
        throw new NotImplementedException();
    }

    public override int CountReactions(string messageId)
    {
        throw new NotImplementedException();
    }

    public override void AddUser(IChatUserProfile user)
    {
        var marshalled = MUserProfile.Marshall(user);
        var collection = _provider.GetCollection<MUserProfile>("user_profiles");
        collection.InsertOne(marshalled);
    }

    public override void AddPersonnel(IChatRoomPersonnel personnel)
    {
        var marshalled = MChatRoomPersonnel.Marshall(personnel);
        var collection = _provider.GetCollection<MChatRoomPersonnel>("chat_rooms_personnel");
        collection.InsertOne(marshalled);
    }

    public override void AddRoom(IChatRoomProfile chatRoom)
    {
        var marshalled = MChatRoomInfo.Marshall(chatRoom);
        var collection = _provider.GetCollection<MChatRoomInfo>("chat_rooms_info");
        collection.InsertOne(marshalled);
    }

    public override void AddInvitation(IChatRoomInvitation invitation)
    {
        var marshalled = MChatRoomInvitation.Marshall(invitation);
        var collection = _provider.GetCollection<MChatRoomInvitation>("chat_room_invitations");
        collection.InsertOne(marshalled);
    }

    public override void AddMessage(IChatMessage message)
    {
        var marshalled = MMessageContent.Marshall(message);
        var collection = _provider.GetCollection<MMessageContent>("message_contents");
        collection.InsertOne(marshalled);
    }

    public override void AddReaction(IChatReaction reaction)
    {
        throw new NotImplementedException();
    }

    public override void UpdateUser(IChatUserProfile user)
    {
        var marshalled = MUserProfile.Marshall(user);
        var collection = _provider.GetCollection<MUserProfile>("user_profiles");
        var filter = Builders<MUserProfile>.Filter.Eq("user_id", user.GetUserId());
        collection.ReplaceOne(filter, marshalled);
    }

    public override void UpdatePersonnel(IChatRoomPersonnel personnel)
    {
        var marshalled = MChatRoomPersonnel.Marshall(personnel);
        var collection = _provider.GetCollection<MChatRoomPersonnel>("chat_rooms_personnel");
        var filter2 = Builders<MChatRoomPersonnel>.Filter.Eq("chat_room_id", personnel.GetRoomId());
        var filter1 = Builders<MChatRoomPersonnel>.Filter.Eq("user_id", personnel.GetUserId());
        collection.ReplaceOne(filter1 & filter2, marshalled);
    }

    public override void UpdateRoom(IChatRoomProfile chatRoom)
    {
        var marshalled = MChatRoomInfo.Marshall(chatRoom);
        var collection = _provider.GetCollection<MChatRoomInfo>("chat_rooms_info");
        var filter = Builders<MChatRoomInfo>.Filter.Eq("room_id", chatRoom.GetRoomId());
        collection.ReplaceOne(filter, marshalled);
    }

    public override void UpdateInvitation(IChatRoomInvitation invitation)
    {
        var marshalled = MChatRoomInvitation.Marshall(invitation);
        var collection = _provider.GetCollection<MChatRoomInvitation>("chat_room_invitations");
        var filter = Builders<MChatRoomInvitation>.Filter.Eq("invitation_string", invitation.GetInvitationId());
        collection.ReplaceOne(filter, marshalled);
    }

    public override void UpdateMessage(IChatMessage message)
    {
        var marshalled = MMessageContent.Marshall(message);
        var collection = _provider.GetCollection<MMessageContent>("message_contents");
        var filter = Builders<MMessageContent>.Filter.Eq("message_id", message.GetMessageId());
        collection.ReplaceOne(filter, marshalled);
    }

    public override void UpdateReaction(IChatReaction reaction)
    {
        throw new NotImplementedException();
    }

    public override int RemoveUser(string userId)
    {
        var collection = _provider.GetCollection<MUserProfile>("user_profiles");
        var filter = Builders<MUserProfile>.Filter.Eq("user_id", userId);
        var result = collection.DeleteOne(filter);
        return (int)result.DeletedCount;
    }

    public override int RemovePersonnel(string userId, string roomId)
    {
        var collection = _provider.GetCollection<MChatRoomPersonnel>("chat_rooms_personnel");
        var filter2 = Builders<MChatRoomPersonnel>.Filter.Eq("chat_room_id", roomId);
        var filter1 = Builders<MChatRoomPersonnel>.Filter.Eq("user_id", userId);
        var result = collection.DeleteOne(filter1 & filter2);
        return (int)result.DeletedCount;
    }

    public override int RemoveRoom(string roomId)
    {
        var collection = _provider.GetCollection<MChatRoomInfo>("chat_rooms_info");
        var filter = Builders<MChatRoomInfo>.Filter.Eq("room_id", roomId);
        var result = collection.DeleteOne(filter);
        return (int)result.DeletedCount;
    }

    public override int RemoveInvitation(string invitationString)
    {
        var collection = _provider.GetCollection<MChatRoomInvitation>("chat_room_invitations");
        var filter = Builders<MChatRoomInvitation>.Filter.Eq("invitation_string", invitationString);
        var result = collection.DeleteOne(filter);
        return (int)result.DeletedCount;
    }

    public override int RemoveMessage(string messageId)
    {
        var collection = _provider.GetCollection<MMessageContent>("message_contents");
        var filter = Builders<MMessageContent>.Filter.Eq("message_id", messageId);
        var result = collection.DeleteOne(filter);
        return (int)result.DeletedCount;
    }

    public override int RemoveReaction(string userId, string messageId)
    {
        throw new NotImplementedException();
    }

    public override void EraseRoom(string roomId)
    {
        OpenTransaction((_, _) =>
        {
            {
                var collection = _provider.GetCollection<MChatRoomInvitation>("chat_room_invitations");
                var filter = Builders<MChatRoomInvitation>.Filter.Eq("chat_room_id", roomId);
                collection.DeleteMany(filter);
            }
            {
                var collection = _provider.GetCollection<MMessageContent>("message_contents");
                var filter = Builders<MMessageContent>.Filter.Eq("room_id", roomId);
                collection.DeleteMany(filter);
            }
            {
                var collection = _provider.GetCollection<MChatRoomPersonnel>("chat_rooms_personnel");
                var filter = Builders<MChatRoomPersonnel>.Filter.Eq("chat_room_id", roomId);
                collection.DeleteMany(filter);
            }
            RemoveRoom(roomId);
        });
    }

    public override void UpdateRoomActivity(string roomId)
    {
        throw new NotImplementedException();
    }

    private MongoDTransaction CreateTransactionInternal() => _provider.CreateTransaction();
    protected override ITransaction CreateTransaction() => CreateTransactionInternal();

    public override void OpenTransaction(Action<ChatAppQuery, ITransaction> action)
    {
        lock (this)
        {
            MongoDTransaction transaction;
            bool isTopLevel;
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
            bool transactionSupported = true;
            try
            {
                try
                {
                    if (isTopLevel) transaction.Start();
                }
                catch (NotSupportedException)
                {
                    transactionSupported = false;
                }
                action(this, transaction);
                if (!isTopLevel) return;
                if (transactionSupported) transaction.Commit();
                transaction.Dispose();
                _currentTransaction = null;
            }
            catch (Exception)
            {
                // If there's an exception, rollback top level transaction, and dispose
                if (transactionSupported) _currentTransaction?.RollBack();
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
            MongoDTransaction transaction;
            bool isTopLevel;
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
            bool transactionSupported = true;
            try
            {
                try
                {
                    if (isTopLevel) transaction.Start();
                }
                catch (NotSupportedException)
                {
                    transactionSupported = false;
                }
                var re = action(this, transaction);
                if (!isTopLevel) return re;
                if (transactionSupported) transaction.Commit();
                transaction.Dispose();
                _currentTransaction = null;
                return re;
            }
            catch (Exception)
            {
                // If there's an exception, rollback top level transaction, and dispose
                if (transactionSupported) _currentTransaction?.RollBack();
                _currentTransaction?.Dispose();
                _currentTransaction = null;
                throw;
            }
        }
    }
}