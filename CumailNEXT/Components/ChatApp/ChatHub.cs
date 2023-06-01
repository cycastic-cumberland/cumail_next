using CumailNEXT.Components.ChatApp.Schemas;
using Microsoft.AspNetCore.SignalR;

namespace CumailNEXT.Components.ChatApp;

public abstract class ChatHub : Hub
{
    protected abstract string? FetchUserId(string idToken);
    protected abstract string? MapRoom(string userId, string roomId);
    protected abstract string? UnmapRoom(string userId, string mappedRoomId);
    protected abstract ChatHubStorage RequestStorage(string connectionId);
    protected abstract Task SendMessageInternal(string userId, MessagePostRequest request);
    protected virtual async Task AddConnectionAsync(string userId, string roomId)
    {
        var connectionId = Context.ConnectionId;
        RequestStorage(connectionId).WriteUsersGroups((users, groups) =>
        {
            users[connectionId] = userId;
            groups[connectionId] = roomId;
        });
        await GroupEnter(roomId, Context.ConnectionId);
    }

    protected virtual void RemoveConnection()
    {
        var connectionId = Context.ConnectionId;
        RequestStorage(connectionId).WriteUsersGroups((users, groups) =>
        {
            users.Remove(connectionId);
            groups.Remove(connectionId);
        });
    }
    protected string? GetUserIdByConnection()
    {
        var connectionId = Context.ConnectionId;
        return RequestStorage(connectionId).ReadUsersGroups((users, _) => users.GetValueOrDefault(connectionId));
    }
    protected string? GetRoomIdByConnection()
    {
        var connectionId = Context.ConnectionId;
        return RequestStorage(connectionId).ReadUsersGroups((_, groups) => groups.GetValueOrDefault(connectionId));
    }

    private async Task RejectConnection(Exception? ex) => await OnDisconnectedAsync(ex);

    private async Task GroupEnter(string groupId, string connection) => await Groups.AddToGroupAsync(connection, groupId);
    
    public override async Task OnConnectedAsync()
    {
        var context = Context.GetHttpContext();
        if (context == null)
        {
            await RejectConnection(new UnauthorizedAccessException("Unauthorized"));
        }
        else
        {
            var idToken = context.Request.Query["idToken"].ToString();
            var roomId  = context.Request.Query["roomId"].ToString();
            try
            {
                var userId = FetchUserId(idToken);
                var cleansedRoomId = MapRoom(userId ?? "", roomId);
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roomId) ||
                    string.IsNullOrEmpty(cleansedRoomId))
                    throw new UnauthorizedAccessException("Unauthorized");
                await AddConnectionAsync(userId, cleansedRoomId);
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        RemoveConnection();
        await base.OnDisconnectedAsync(exception);
    }

    public void SendMessage(MessagePostRequest request)
    {
        var userId = GetUserIdByConnection();
        if (userId == null) return;
        SendMessageInternal(userId, request);
    }
}