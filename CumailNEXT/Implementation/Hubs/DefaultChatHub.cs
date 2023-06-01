using CumailNEXT.Components.Auth;
using CumailNEXT.Components.ChatApp;
using CumailNEXT.Components.ChatApp.Schemas;
using CumailNEXT.Implementation.ChatApp;
using Microsoft.AspNetCore.SignalR;

namespace CumailNEXT.Implementation.Hubs;

public class DefaultChatHub : ChatHub
{
    // private static readonly ChatAppQuery Query = new PostgresChatAppQuery(Engine.PostgresConnectionSettings);
    // private static readonly AuthProvider Auth = Engine.Auth;
    private QueuedChatHubStorage GetStorage() => Core.Engine.HubStorageFactory.CreateInstance();
    protected override ChatHubStorage RequestStorage(string connectionId) => GetStorage();
    protected override string? FetchUserId(string idToken)
    {
        try
        {
            var authUser = GetStorage().Auth.GetUserByIdToken(new AuthToken(idToken));
            // Stray queries
            using var appQuery = GetStorage().GetQuery(Context.ConnectionId);
            return appQuery.GetUserById(authUser.UserUuid)?.GetUserId();
        }
        catch (Exception)
        {
            return null;
        }
    }

    // No scratch that. Fuck it we ball
    //
    // protected override async Task AddConnectionAsync(string userId, string roomId)
    // {
    //     // Group record must be created before the query is created;
    //      await base.AddConnectionAsync(userId, roomId);
    //      GetStorage().AddConnection(Context.ConnectionId);
    // }
    //
    // protected override void RemoveConnection()
    // {
    //     // Vice versa, the query must be destroyed before the record is erased
    //     GetStorage().RemoveConnection(Context.ConnectionId);
    //     base.RemoveConnection();
    // }

    protected override async Task SendMessageInternal(string userId, MessagePostRequest request)
    {
        var groupName = GetRoomIdByConnection();
        if (groupName == null) return;
        var roomId = UnmapRoom(userId, groupName);
        if (roomId == "") return;
        var chatMessage = new ChatMessage(userId, roomId, request.MessageContent, request.ClientStamp);
        // GetStorage().Engine.Dispatch(() => GetStorage().Query.AddMessageSafe(chatMessage));
        using var query = GetStorage().GetQuery(Context.ConnectionId);
        query.AddMessage(chatMessage);
        await Clients.Group(groupName).SendAsync("ReceiveMessage", chatMessage);
    }

    protected override string? MapRoom(string userId, string roomId)
    {
        try
        {
            // Stray queries
            using var appQuery = GetStorage().GetQuery(Context.ConnectionId);
            var personnel = appQuery.GetPersonnelByUserAndRoomId(userId, roomId);
            return personnel == null ? null : $"mapped:{roomId}";
        }
        catch (Exception)
        {
            return null;
        }
    }

    protected override string UnmapRoom(string userId, string mappedRoomId)
    {
        return mappedRoomId.Length > 7 ? mappedRoomId[7..] : "";
    }
}