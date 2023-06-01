using CumailNEXT.Components.Auth;
using CumailNEXT.Components.ChatApp.Schemas;

namespace CumailNEXT.Components.ChatApp;

public abstract class ChatAppInstance : IDisposable
{
    protected readonly AuthToken Auth;
    protected readonly AuthProvider Provider;
    private readonly object _tokenLock = new double();
    private string? _bareToken = null;

    protected string InstigatorId
    {
        get
        {
            lock (_tokenLock)
            {
                _bareToken ??= Provider.GetUserIdFromToken(Auth);
            }

            return _bareToken;
        }
    }
    protected ChatAppInstance(AuthProvider provider, AuthToken auth)
    {
        Provider = provider;
        Auth = auth;
    }
    
    // CREATE actions
    public abstract void CreateNewUser(IChatUserProfile userProfile);
    public abstract void CreateChatRoom(string roomName);
    // public abstract void CreateUserAffiliation(IChatRoomPersonnel personnel);
    public abstract void JoinRoom(string invitationId);
    public abstract void CreateInvitation(IChatRoomInvitation invitation);

    // READ actions
    public abstract IChatUserProfile GetUserProfile(string targetUserId);
    public abstract IChatRoomProfile GetChatRoomProfile(string targetRoomId);
    public abstract List<IChatRoomPersonnel> GetAffiliatedRooms(string userId);
    public abstract List<IChatRoomPersonnel> GetAffiliatedUsers(string roomId);
    public abstract List<IChatRoomInvitation> GetInvitations(string roomId);
    public abstract IChatRoomPersonnel GetUserAffiliation(PersonnelInquiryRequest request);
    public abstract IChatRoomInvitation GetInvitation(string invitationString);
    public abstract MessageGetResponse GetMessageSlice(MessageGetRequest request);
    
    // UPDATE actions
    // Expected to be PartialDataclass
    public abstract void UpdateUserProfile(IChatUserProfile newProfile);
    public abstract void DisableUser(string userId);
    public abstract void EnableUser(string userId);
    public abstract void UpdateChatRoomProfile(IChatRoomProfile newProfile);
    public abstract void UpdateUserRole(IChatRoomPersonnel personnel);
    public abstract void UpdateInvitation(IChatRoomInvitation invitation);
    
    // DELETE actions
    public abstract void DeleteUser(string userId);
    public abstract void DeleteRoom(string roomId);
    public abstract void DeleteUserFromRoom(PersonnelInquiryRequest request);
    public abstract void DeleteInvitation(string invitationString);
    public abstract void Dispose();
}

public abstract class AgentBasedChatAppFactory
{
    public abstract ChatAppInstance CreateInstance(AuthToken auth);
}