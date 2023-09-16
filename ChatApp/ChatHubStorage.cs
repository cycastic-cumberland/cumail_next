using CoreComponents.Paralex;

namespace ChatApp;

public class ChatHubStorage
{
    private readonly ReadWriteLock<Dictionary<string, string>, Dictionary<string, string>> _usersGroupsMap = new(new(), new());

    public virtual TR ReadUsersGroups<TR>(Func<Dictionary<string, string>, Dictionary<string, string>, TR> action)
        => _usersGroupsMap.Read(action);
    public virtual TR WriteUsersGroups<TR>(Func<Dictionary<string, string>, Dictionary<string, string>, TR> action)
        => _usersGroupsMap.Write(action);
    public virtual void ReadUsersGroups(Action<Dictionary<string, string>, Dictionary<string, string>> action)
        => _usersGroupsMap.Read(action);
    public virtual void WriteUsersGroups(Action<Dictionary<string, string>, Dictionary<string, string>> action)
        => _usersGroupsMap.Write(action);
}