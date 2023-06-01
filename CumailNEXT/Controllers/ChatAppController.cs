using CumailNEXT.Components.Auth;
using CumailNEXT.Components.ChatApp.Schemas;
using CumailNEXT.Implementation.ChatApp.Exceptions;
using CumailNEXT.Implementation.ChatApp.Tables;
using CumailNEXT.Implementation.Core;
using Microsoft.AspNetCore.Mvc;

namespace CumailNEXT.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatAppController : ControllerBase
{
    private AuthToken? Authorization
    {
        get
        {
            IHeaderDictionary headers = Request.Headers;
            string? authorizationHeader = headers.Authorization;
            authorizationHeader ??= "";
            if (!authorizationHeader.StartsWith("Bearer ")) return null;
            string authorization = authorizationHeader[7..];
            return new AuthToken
            {
                IdToken = authorization
            };
        }
    }
    [HttpGet("test")]
    public IActionResult HelloWorld()
    {
        return Ok("Ohayo, Sekai");
    }
    //////////////////////////////////////////////////////////////////////
    //                              CREATE                              //
    //////////////////////////////////////////////////////////////////////
    [HttpPost("create_room")]
    public IActionResult CreateRoom(Dictionary<string, string> json)
    {
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            appInstance.CreateChatRoom(json["roomName"]);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (KeyNotFoundException)
        {
            return BadRequest("INVALID_ROOM_NAME");
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    [HttpPost("join_room")]
    public IActionResult JoinRoom(Dictionary<string, string> json)
    {
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            appInstance.JoinRoom(json["invitationId"]);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidTokenException)
        {
            return Unauthorized();
        }
        catch (KeyNotFoundException)
        {
            return BadRequest("INVALID_INVITATION_NAME");
        }
        catch (AlreadyJoinedException)
        {
            return BadRequest("ALREADY_JOINED_ROOM");
        }
        catch (NotFoundException)
        {
            return BadRequest("ROOM_NOT_FOUND");
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    [HttpPost("create_invitation")]
    public IActionResult CreateInvitation(ChatRoomInvitation invitation)
    {
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            appInstance.CreateInvitation(invitation);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidDataException)
        {
            return BadRequest("EMPTY_INVITATION");
        }
        catch (InvitationExistedException)
        {
            return StatusCode(403, "INVITATION_EXISTED");
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    //////////////////////////////////////////////////////////////////////
    //                               READ                               //
    //////////////////////////////////////////////////////////////////////
    [HttpGet("user")]
    public IActionResult GetUserProfile()
    {
        var requestedUserId = Request.Query["id"].ToString();
        // Allow non-logged in users to read profiles
        using var appInstance = Engine.ChatAppFactory.CreateInstance(new AuthToken());
        try
        {
            return Ok(appInstance.GetUserProfile(requestedUserId));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    [HttpGet("joined_room")]
    public IActionResult GetAffiliatedRooms()
    {
        var requestedUserId = Request.Query["userId"].ToString();
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            return Ok(appInstance.GetAffiliatedRooms(requestedUserId).Select(v => (ChatRoomPersonnel)v).ToList());
        }
        catch (InvalidTokenException)
        {
            return Unauthorized();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    [HttpGet("room_members")]
    public IActionResult GetRoomMembers()
    {
        var requestedRoomId = Request.Query["roomId"].ToString();
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            return Ok(appInstance.GetAffiliatedUsers(requestedRoomId).Select(v => (ChatRoomPersonnel)v).ToList());
        }
        catch (InvalidTokenException)
        {
            return Unauthorized();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    [HttpGet("invitations")]
    public IActionResult GetInvitations()
    {
        var requestedRoomId = Request.Query["roomId"].ToString();
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            return Ok(appInstance.GetInvitations(requestedRoomId).Select(v => (ChatRoomInvitation)v).ToList());
        }
        catch (InvalidTokenException)
        {
            return Unauthorized();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    [HttpGet("room")]
    public IActionResult GetChatRoomProfile()
    {
        var requestedRoomId = Request.Query["id"].ToString();
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            return Ok(appInstance.GetChatRoomProfile(requestedRoomId));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    [HttpGet("personnel")]
    public IActionResult GetAffiliation()
    {
        var requestedUserId = Request.Query["userId"].ToString();
        var requestedRoomId = Request.Query["roomId"].ToString();
        var inquiry = new PersonnelInquiryRequest
        {
            UserId = requestedUserId,
            RoomId = requestedRoomId
        };
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            return Ok(appInstance.GetUserAffiliation(inquiry));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    [HttpGet("messages")]
    public IActionResult GetMessageSlice()
    {
        var requestedSliceEnd = Request.Query["endAt"].ToString();
        var requestedRoomId   = Request.Query["roomId"].ToString();
        var isAscending       = Request.Query["order"].ToString();

        int end;
        try
        {
            end = int.Parse(requestedSliceEnd);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        var inquiry = new MessageGetRequest
        {
            RoomId = requestedRoomId,
            End = end,
            IsAscending = isAscending == "1"
        };
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            return Ok(appInstance.GetMessageSlice(inquiry));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    //////////////////////////////////////////////////////////////////////
    //                              UPDATE                              //
    //////////////////////////////////////////////////////////////////////
    [HttpPatch("change_room_name")]
    public IActionResult ChangeRoomName(Dictionary<string, string> request)
    {
        PartialChatRoomProfile partialRoomProfile;
        try
        {
            partialRoomProfile  = new PartialChatRoomProfile(
                new { RoomId = request["roomId"], 
                    RoomName = request["roomName"] });
        }
        catch (KeyNotFoundException)
        {
            return BadRequest();
        }
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            appInstance.UpdateChatRoomProfile(partialRoomProfile);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    //////////////////////////////////////////////////////////////////////
    //                              DELETE                              //
    //////////////////////////////////////////////////////////////////////
    [HttpDelete("remove_from_room")]
    public IActionResult RemoveFromRoom()
    {
        var requestedUserId = Request.Query["userId"].ToString();
        var requestedRoomId = Request.Query["roomId"].ToString();
        var inquiry = new PersonnelInquiryRequest
        {
            UserId = requestedUserId,
            RoomId = requestedRoomId
        };
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            appInstance.DeleteUserFromRoom(inquiry);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
    [HttpDelete("remove_invitation")]
    public IActionResult RemoveInvitation()
    {
        var invitation = Request.Query["inv"].ToString();
        var token = Authorization;
        if (token == null) return Unauthorized();
        using var appInstance = Engine.ChatAppFactory.CreateInstance(token);
        try
        {
            appInstance.DeleteInvitation(invitation);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception)
        {
            return StatusCode(500, "UNHANDLED_EXCEPTION");
        }
    }
}