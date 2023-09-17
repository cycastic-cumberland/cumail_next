using Auth;
using ChatApp;
using CoreComponents.Paralex;
using PostgresChatApp.ChatApp;

namespace CumailNEXT.Implementation.ChatApp;

// A class for delegating requests
public class QueuedChatHubStorageInstance : QueuedChatHubStorage
{
    private readonly QueuedChatHubStorage _baseInstance;
    public QueuedChatHubStorageInstance(QueuedChatHubStorage baseInstance)
    {
        _baseInstance = baseInstance;
    }
    public override AuthProvider Auth => _baseInstance.Auth;
    public override void AddConnection(string connectionId) => _baseInstance.AddConnection(connectionId);
    public override ChatAppQuery GetQuery(string connectionId) => _baseInstance.GetQuery(connectionId);
    public override void RemoveConnection(string connectionId) => _baseInstance.RemoveConnection(connectionId);
    public override TR ReadUsersGroups<TR>(Func<Dictionary<string, string>, Dictionary<string, string>, TR> action)
        => _baseInstance.ReadUsersGroups(action);
    public override void ReadUsersGroups(Action<Dictionary<string, string>, Dictionary<string, string>> action)
        => _baseInstance.ReadUsersGroups(action);
    public override TR WriteUsersGroups<TR>(Func<Dictionary<string, string>, Dictionary<string, string>, TR> action)
        => _baseInstance.WriteUsersGroups(action);
    public override void WriteUsersGroups(Action<Dictionary<string, string>, Dictionary<string, string>> action)
        => _baseInstance.WriteUsersGroups(action);
}

public class NotQueuedChatHubStorage : QueuedChatHubStorage
{
    public override void AddConnection(string connectionId) => throw new NotImplementedException();
    public override ChatAppQuery GetQuery(string connectionId) => Core.Engine.NewAppQuery;
    public override void RemoveConnection(string connectionId) => throw new NotImplementedException();
}

public class QueuedChatHubStorage : ChatHubStorage
{
    // 1st map: GroupId => PostgresChatAppQuery
    // 2nd map: GroupId => reference count
    private readonly ReadWriteLock<Dictionary<string, ChatAppQuery>, Dictionary<string, long>> _connectionMap = new(new(), new());
    public QueuedChatHubStorage CreateInstance() => new QueuedChatHubStorageInstance(this);
    
    public virtual AuthProvider Auth => Core.Engine.Auth;

    private void AddConnectionToGroup(string groupId)
        => _connectionMap.Write((queriesMap, referencesMap) =>
        {
            queriesMap.TryAdd(groupId, Core.Engine.NewAppQuery);
            if (!referencesMap.ContainsKey(groupId))
            {
                referencesMap[groupId] = 1;
            }
            else
            {
                referencesMap[groupId] += 1;
            }
        });

    // Sacrificing durability check for faster speed
    // As long as my code is good, there's no need to worry about this
    // (Spoiler alert: my code isn't good)
    private ChatAppQuery GetQueryByGroup(string groupId)
        => _connectionMap.Read((queriesMap, _) => queriesMap[groupId]);
    private void RemoveConnectionFromGroup(string groupId)
        => _connectionMap.Write((queriesMap, referencesMap) =>
        {
            // Make sure that the connection is recycled if its reference count hit zero
            // (No one is listening on this group)
            //
            // Durability failsafe:
            // Due to being a storage object, this method must have some failsafe
            // put in place
            if (referencesMap.ContainsKey(groupId))
            {
                var newCount = referencesMap[groupId] - 1;
                referencesMap[groupId] = newCount;
                if (newCount > 0) return;
                referencesMap.Remove(groupId);
                queriesMap.Remove(groupId);
            }
            else
            {
                queriesMap.Remove(groupId);
            }
        });

    public virtual void AddConnection(string connectionId)
    {
        var groupId = "";
        ReadUsersGroups((_, groupsMap) =>
        {
            groupId = groupsMap[connectionId];
        });
        AddConnectionToGroup(groupId);
    }

    public virtual ChatAppQuery GetQuery(string connectionId)
    {
        var groupId = "";
        // Hubs will try to read general infos before creating records
        // Create some stray hubs that get cleaned up manually
        try
        {
            ReadUsersGroups((_, groupsMap) =>
            {
                groupId = groupsMap[connectionId];
            });
        }
        catch (Exception)
        {
            return Core.Engine.NewAppQuery;
        }
        return GetQueryByGroup(groupId);
    }
    public virtual void RemoveConnection(string connectionId)
    {
        var groupId = "";
        ReadUsersGroups((_, groupsMap) =>
        {
            groupId = groupsMap[connectionId];
        });
        RemoveConnectionFromGroup(groupId);
    }
}