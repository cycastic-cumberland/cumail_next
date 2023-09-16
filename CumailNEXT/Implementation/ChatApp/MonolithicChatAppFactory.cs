using Auth;
using ChatApp;
using ChatApp.Exceptions;
using ChatApp.Schemas;
using ChatApp.Tables;
using CoreComponents.Core;
using ExtendedComponents.Core;

namespace CumailNEXT.Implementation.ChatApp;

public class MonolithicChatAppInstance : ChatAppInstance
{
    private readonly ChatAppQuery _query;
    public MonolithicChatAppInstance(ChatAppQuery query, AuthProvider provider, AuthToken auth)
        : base(provider, auth)
    {
        _query = query;
    }
    
    private static void SafeBlock(Action action)
    {
        try
        {
            action();
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    public override void CreateNewUser(IChatUserProfile userProfile)
    {
        throw new NotImplementedException();
    }

    public override void CreateChatRoom(string roomName)
    {
        var roomNameMaxLength = ProjectSettings.Instance.Get(SettingsCatalog.ChatRoomNameMaxLength, 32);
        if (roomName.Length > roomNameMaxLength) throw new StringTooLongException();
        var creatorId = InstigatorId;
        var roomId = Crypto.HashSha256String(
            $"${creatorId}" +
            $"${roomName}" +
            $"${DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}" +
            $"${Crypto.GenerateSecureString(32)}");
        try
        {
            _query.OpenTransaction((baseQuery, _) =>
            {
                baseQuery.AddRoom(new ChatRoomInfo
                {
                    RoomId = roomId,
                    RoomName = roomName,
                    Description = "",
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    LastActivity = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    IsPublic = true,
                });
                baseQuery.AddPersonnel(new ChatRoomPersonnel
                {
                    UserId = creatorId,
                    ChatRoomId = roomId,
                    Role = 5
                });
            });
        }
        catch (Exception)
        {
            throw new NullReferenceException();
        }
    }

    public override void JoinRoom(string invitationId)
    {
        var targetUser = InstigatorId;
        _query.OpenTransaction((baseQuery, _) =>
        {
            var invitation = baseQuery.GetInvitationById(invitationId);
            if (invitation == null || !invitation.IsEnabled()) throw new NotFoundException("invitation");
            // Already joined
            if (baseQuery.GetPersonnelByUserAndRoomId(targetUser, invitation.GetRoomId()) != null)
                throw new AlreadyJoinedException();
            // Valid invitation
            baseQuery.AddPersonnel(new ChatRoomPersonnel
            {
                UserId = targetUser,
                ChatRoomId = invitation.GetRoomId(),
                Role = 1
            });
        });
    }
    
    public override void CreateInvitation(IChatRoomInvitation invitation)
    {
        if (string.IsNullOrWhiteSpace(invitation.GetInvitationId()) ||
            string.IsNullOrEmpty(invitation.GetInvitationId()))
            throw new InvalidDataException();
        if (InstigatorId != invitation.GetInstigatorId()) throw new UnauthorizedAccessException();
        _query.OpenTransaction((baseQuery, _) =>
        {
            // Get Requester affiliation
            var affiliation = baseQuery.GetPersonnelByUserAndRoomId(InstigatorId, invitation.GetRoomId());
            if (affiliation == null || affiliation.GetRole() < 3) return;
            try
            {
                baseQuery.AddInvitation(invitation);
            }
            catch (Exception)
            {
                throw new InvitationExistedException();
            }
        });
    }

    public override IChatUserProfile GetUserProfile(string targetUserId)
    {
        // Must be logged in
        // EDIT: Scratched that
        // var unused = InstigatorId;
        string instigator;
        try
        {
            instigator = InstigatorId;
        }
        catch (Exception)
        {
            instigator = "";
        }
        return _query.OpenTransaction((baseQuery, _) =>
        {
            var user = baseQuery.GetUserById(targetUserId);
            if (user == null) return null;
            if (user.GetUserId() != instigator && user.IsDisabled()) return null;
            return user;
        }) ?? throw new NotFoundException("user");
    }

    public override List<IChatRoomPersonnel> GetAffiliatedRooms(string userId)
    {
        return _query.GetPersonnelByUserId(userId);
    }

    public override List<IChatRoomPersonnel> GetAffiliatedUsers(string roomId)
    {
        return _query.GetPersonnelByRoomId(roomId);
    }

    public override List<IChatRoomInvitation> GetInvitations(string roomId)
    {
        var instigator = InstigatorId;
        return _query.OpenTransaction((baseQuery, _) =>
        {
            var affiliation = baseQuery.GetPersonnelByUserAndRoomId(instigator, roomId);
            if (affiliation == null || affiliation.GetRole() < 4) throw new UnauthorizedAccessException();
            return baseQuery.GetInvitationsByRoomId(roomId);
        });
    }
    public override IChatRoomProfile GetChatRoomProfile(string targetRoomId)
    {
        return _query.OpenTransaction((baseQuery, _) =>
        {
            int role;
            try
            {
                var affiliation = baseQuery.GetPersonnelByUserAndRoomId(InstigatorId, targetRoomId);
                role = affiliation?.GetRole() ?? -1;
            }
            catch (Exception)
            {
                role = -1;
            }
            var roomProfile = baseQuery.GetChatRoomById(targetRoomId);
            if (roomProfile == null) return null;
            roomProfile.SetHashedPassword("");
            // If room is not available to public and user is not signed in => don't return anything;
            return !roomProfile.IsVisible() && role == -1 ? null : roomProfile;
        }) ?? throw new NotFoundException("room");
    }

    public override IChatRoomPersonnel GetUserAffiliation(PersonnelInquiryRequest request)
    {
        return _query.OpenTransaction((baseQuery, _) =>
        {
            int role;
            try
            {
                var affiliation = baseQuery.GetPersonnelByUserAndRoomId(InstigatorId, request.RoomId);
                role = affiliation?.GetRole() ?? -1;
            }
            catch (Exception)
            {
                role = -1;
            }
            var roomProfile = baseQuery.GetChatRoomById(request.RoomId);
            if (roomProfile == null) return null;
            // If room is not available to public and user is not signed in => don't return anything;
            if (!roomProfile.IsVisible() && role == -1) return null;
            return baseQuery.GetPersonnelByUserAndRoomId(request.UserId, request.RoomId) ??
                   throw new NotFoundException("personnel");
        }) ?? throw new NotFoundException("room");
    }

    public override IChatRoomInvitation GetInvitation(string invitationString)
    {
        var invitation = _query.GetInvitationById(invitationString) ?? throw new NotFoundException("invitation");
        invitation.SetInstigatorId("");
        return invitation.IsEnabled() ? invitation : throw new NotFoundException("invitation");
    }

    public override MessageGetResponse GetMessageSlice(MessageGetRequest request)
    {
        return _query.OpenTransaction((baseQuery, _) =>
        {
            int role;
            try
            {
                var affiliation = baseQuery.GetPersonnelByUserAndRoomId(InstigatorId, request.RoomId);
                role = affiliation?.GetRole() ?? -1;
            }
            catch (Exception)
            {
                role = -1;
            }

            if (role == -1) throw new UnauthorizedAccessException();
            var roomProfile = baseQuery.GetChatRoomById(request.RoomId);
            if (roomProfile == null) return null;
            var messages = baseQuery.GetMessageBySlice(request.RoomId, request.End, request.IsAscending ? ChatAppQuery.Order.Ascending : ChatAppQuery.Order.Descending);
            var re = new MessageGetResponse
            {
                Messages = messages.Select(v => (ChatMessage)v).ToList()
            };
            return re;
        }) ?? new MessageGetResponse();
    }

    // Partial Dataclasses
    public override void UpdateUserProfile(IChatUserProfile newProfile)
    {
        try
        {
            if (InstigatorId != newProfile.GetUserId()) throw new UnauthorizedAccessException();
        }
        catch (InvalidCastException)
        {
            throw new KeyNotFoundException();
        }
        catch (Exception)
        {
            throw new UnauthorizedAccessException();
        }

        _query.OpenTransaction((baseQuery, _) =>
        {
            var reconstructed = baseQuery.GetUserById(InstigatorId) as UserProfile ?? throw new Exception("UNEXPECTED");
            SafeBlock(() => reconstructed.UserName = newProfile.GetUserName());
            SafeBlock(() => reconstructed.ProfilePictureUrl = newProfile.GetPfpUrl());
            SafeBlock(() => reconstructed.Description = newProfile.GetDescription());
            // Not allowed
            // TransactionBlock(() => reconstructed.Disabled = newProfile.IsDisabled());
            // TransactionBlock(() => reconstructed.Deleted = newProfile.IsDeleted());
            baseQuery.UpdateUser(newProfile);
        });
    }

    public override void DisableUser(string userId)
    {
        _query.OpenTransaction((baseQuery, _) =>
        {
            if (baseQuery.GetUserById(userId) is not UserProfile targetUser) return;
            targetUser.Disabled = true;
            baseQuery.UpdateUser(targetUser);
        });
    }
    
    public override void EnableUser(string userId)
    {
        _query.OpenTransaction((baseQuery, _) =>
        {
            if (baseQuery.GetUserById(userId) is not UserProfile targetUser) return;
            targetUser.Disabled = false;
            baseQuery.UpdateUser(targetUser);
        });
    }

    public override void UpdateChatRoomProfile(IChatRoomProfile newProfile)
    {
        _query.OpenTransaction((baseQuery, _) =>
        {
            var affiliation = baseQuery.GetPersonnelByUserAndRoomId(InstigatorId, newProfile.GetRoomId());
            if (affiliation == null || affiliation.GetRole() < 4) throw new UnauthorizedAccessException();
            var reconstructed = baseQuery.GetChatRoomById(newProfile.GetRoomId()) as ChatRoomInfo ?? throw new NotFoundException("ID");
            SafeBlock(() => reconstructed.RoomName = newProfile.GetRoomName());
            SafeBlock(() => reconstructed.Description = newProfile.GetDescription());
            SafeBlock(() => reconstructed.IsPublic = newProfile.IsVisible());
            baseQuery.UpdateRoom(reconstructed);
        });
    }

    public override void UpdateUserRole(IChatRoomPersonnel personnel)
    {
        // Don't allow user to be promoted to Creator
        if (personnel.GetRole() == 5) throw new UnauthorizedAccessException();
        _query.OpenTransaction((baseQuery, _) =>
        {
            // Don't change Creator's role
            var targetAffiliation = baseQuery.GetPersonnelByUserAndRoomId(personnel.GetUserId(), personnel.GetRoomId());
            if (targetAffiliation == null || targetAffiliation.GetRole() == 5) throw new UnauthorizedAccessException();
            // Only Creators are allowed to alter roles
            var instigatorAffiliation = baseQuery.GetPersonnelByUserAndRoomId(InstigatorId, personnel.GetRoomId());
            if (instigatorAffiliation == null || instigatorAffiliation.GetRole() != 5) throw new UnauthorizedAccessException();
            // Go through if no problem
            baseQuery.UpdatePersonnel(personnel);
        });
    }

    public override void UpdateInvitation(IChatRoomInvitation invitation)
    {
        var instigator = InstigatorId;
        _query.OpenTransaction((baseQuery, _) =>
        {
            var affiliation = baseQuery.GetPersonnelByUserAndRoomId(instigator, invitation.GetRoomId());
            if (affiliation == null || affiliation.GetRole() < 3) throw new UnauthorizedAccessException();
            // var reconstructed 
        });
    }
    // Ran out of time
    //
    // Fuck it we ball
    //
    public override void DeleteUser(string userId)
    {
        throw new NotImplementedException();
    }

    public override void DeleteRoom(string roomId)
    {
        throw new NotImplementedException();
    }

    public override void DeleteUserFromRoom(PersonnelInquiryRequest request)
    {
        var instigator = InstigatorId;
        _query.OpenTransaction((baseQuery, _) =>
        {
            var instigatorAffiliation = baseQuery.GetPersonnelByUserAndRoomId(instigator, request.RoomId);
            var targetAffiliation = baseQuery.GetPersonnelByUserAndRoomId(request.UserId, request.RoomId);
            if (targetAffiliation != null && instigator == request.UserId)
            {
                baseQuery.RemovePersonnel(request.UserId, request.RoomId);
                return;
            }
            if (targetAffiliation == null || instigatorAffiliation == null || instigatorAffiliation.GetRole() < 4) throw new UnauthorizedAccessException();
            if (instigatorAffiliation.GetRole() < targetAffiliation.GetRole()) throw new UnauthorizedAccessException();
            baseQuery.RemovePersonnel(request.UserId, request.RoomId);
        });
    }

    public override void DeleteInvitation(string invitationString)
    {
        var instigator = InstigatorId;
        _query.OpenTransaction((baseQuery, _) =>
        {
            var invitation = baseQuery.GetInvitationById(invitationString);
            if (invitation == null) throw new NotFoundException("INVITATION");
            var instigatorAffiliation = baseQuery.GetPersonnelByUserAndRoomId(instigator, invitation.GetRoomId());
            if (instigatorAffiliation == null || instigatorAffiliation.GetRole() < 4)
                throw new UnauthorizedAccessException();
            baseQuery.RemoveInvitation(invitationString);
        });
    }

    public override void Dispose() => _query.Dispose();
}

public class MonolithicChatAppFactory : AgentBasedChatAppFactory
{
    private readonly AuthProvider _provider;
    private readonly ChatAppQueryFactory _agent;

    public MonolithicChatAppFactory(AuthProvider provider, ChatAppQueryFactory agent)
    {
        _provider = provider;
        _agent = agent;
    }

    public override ChatAppInstance CreateInstance(AuthToken auth)
    {
        return new MonolithicChatAppInstance(_agent.CreateInstance(), _provider, auth);
    }
}